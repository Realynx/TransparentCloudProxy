#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "Please run as root (use sudo)."
  exit 1
fi

if ! command -v curl >/dev/null 2>&1 || ! command -v unzip >/dev/null 2>&1 || ! command -v tar >/dev/null 2>&1; then
  apt-get update -y
  apt-get install -y curl unzip tar
fi

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  apt-get update -y
  apt-get install -y nodejs npm
fi

REPO="Realynx/TransparentCloudProxy"
release_json="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest")"
ARCHIVE_URL="$(printf '%s' "${release_json}" | sed -n 's/.*"browser_download_url": "\([^"]*reactserver\.zip\)".*/\1/p' | head -n1)"

if [[ -z "${ARCHIVE_URL}" ]]; then
  ARCHIVE_URL="$(printf '%s' "${release_json}" | sed -n 's/.*"browser_download_url": "\([^"]*reactserver\.tar\.gz\)".*/\1/p' | head -n1)"
fi

if [[ -z "${ARCHIVE_URL}" ]]; then
  echo "Could not find reactserver release asset (.zip or .tar.gz)."
  exit 1
fi

install_root="/opt/realynx-reactserver"
archive_path="/tmp/reactserver"

mkdir -p "${install_root}"
rm -rf "${install_root:?}"/*

if [[ "${ARCHIVE_URL}" == *.zip ]]; then
  archive_path+=".zip"
  curl -fL "${ARCHIVE_URL}" -o "${archive_path}"
  unzip -o "${archive_path}" -d "${install_root}"
else
  archive_path+=".tar.gz"
  curl -fL "${ARCHIVE_URL}" -o "${archive_path}"
  tar -xzf "${archive_path}" -C "${install_root}"
fi

mkdir -p "${install_root}/server" "${install_root}/client"
if [[ -d "${install_root}/server-dist" ]]; then
  rm -rf "${install_root}/server/dist"
  mv "${install_root}/server-dist" "${install_root}/server/dist"
fi

if [[ -d "${install_root}/client-dist" ]]; then
  rm -rf "${install_root}/client/dist"
  mv "${install_root}/client-dist" "${install_root}/client/dist"
fi

if [[ -f "${install_root}/.env.example" && ! -f "${install_root}/.env" ]]; then
  cp "${install_root}/.env.example" "${install_root}/.env"
fi

cd "${install_root}"
npm ci --omit=dev

setup_service=""
if [[ -t 0 ]]; then
  while true; do
    read -r -p "Setup a systemd service to run ReactServer at startup? [y/N]: " response
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
  cat >/etc/systemd/system/realynx-reactserver.service <<'SERVICE'
[Unit]
Description=Realynx ReactServer
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/realynx-reactserver
ExecStart=/usr/bin/env npm start
Restart=always
RestartSec=5
Environment=NODE_ENV=production
EnvironmentFile=-/opt/realynx-reactserver/.env

[Install]
WantedBy=multi-user.target
SERVICE

  systemctl daemon-reload
  systemctl enable --now realynx-reactserver

  echo "Installed and started realynx-reactserver.service"
  printf '\nRecent startup logs:\n\n'
  sleep 2
  journalctl -u realynx-reactserver -n 80 --no-pager || true
else
  echo "Install complete. Systemd service was not configured."
  echo "Run manually: cd ${install_root} && npm start"
fi
