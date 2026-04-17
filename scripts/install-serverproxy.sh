#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "Please run as root (use sudo)."
  exit 1
fi

if ! command -v curl >/dev/null 2>&1 || ! command -v tar >/dev/null 2>&1 || ! command -v unzip >/dev/null 2>&1; then
  apt-get update -y
  apt-get install -y curl tar unzip
fi

REPO="Realynx/TransparentCloudProxy"
release_json="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest")"
ARCHIVE_URL="$(printf '%s' "${release_json}" | sed -n 's/.*"browser_download_url": "\([^"]*serverproxy-linux-x64\.zip\)".*/\1/p' | head -n1)"

if [[ -z "${ARCHIVE_URL}" ]]; then
  ARCHIVE_URL="$(printf '%s' "${release_json}" | sed -n 's/.*"browser_download_url": "\([^"]*serverproxy-linux-x64\.tar\.gz\)".*/\1/p' | head -n1)"
fi

if [[ -z "${ARCHIVE_URL}" ]]; then
  echo "Could not find serverproxy-linux-x64 release asset (.zip or .tar.gz)."
  exit 1
fi

install_root="/opt/realynx-serverproxy"
bin_dir="${install_root}/bin"
archive_path="/tmp/serverproxy-linux-x64"

mkdir -p "${bin_dir}"
rm -rf "${bin_dir:?}"/*

if [[ "${ARCHIVE_URL}" == *.zip ]]; then
  archive_path+=".zip"
  curl -fL "${ARCHIVE_URL}" -o "${archive_path}"
  unzip -o "${archive_path}" -d "${bin_dir}"
else
  archive_path+=".tar.gz"
  curl -fL "${ARCHIVE_URL}" -o "${archive_path}"
  tar -xzf "${archive_path}" -C "${bin_dir}"
fi

chmod +x "${bin_dir}/TransparentCloudServerProxy.WebDashboard"

setup_service=""
if [[ -t 0 ]]; then
  while true; do
    read -r -p "Setup a systemd service to run proxy at startup? [y/N]: " response
    case "${response}" in
      [yY]|[yY][eE][sS])
        setup_service="yes"
        break
        ;;
      ""|[nN]|[nN][oO])
        setup_service="no"
        break
        ;;
      *)
        echo "Please answer yes or no."
        ;;
    esac
  done
else
  setup_service="yes"
fi

if [[ "${setup_service}" == "yes" ]]; then
  cat >/etc/systemd/system/realynx-serverproxy.service <<'SERVICE'
[Unit]
Description=Realynx ServerProxy
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/realynx-serverproxy/bin
ExecStart=/opt/realynx-serverproxy/bin/TransparentCloudServerProxy.WebDashboard
Restart=always
RestartSec=5
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
Environment=Kestrel__Endpoints__Http__Url=http://0.0.0.0:8080
Environment=Kestrel__Endpoints__Https__Url=http://127.0.0.1:0

[Install]
WantedBy=multi-user.target
SERVICE

  systemctl daemon-reload
  systemctl enable --now realynx-serverproxy

  echo "Installed and started realynx-serverproxy.service"
  printf '\nRecent startup logs (look for '\''OneKey Pass'\''):\n\n'
  sleep 2
  journalctl -u realynx-serverproxy -n 80 --no-pager || true
else
  echo "Install complete. Systemd service was not configured."
  echo "Run manually: ${bin_dir}/TransparentCloudServerProxy.WebDashboard"
fi
