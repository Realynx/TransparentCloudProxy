#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
  echo "Please run as root (use sudo)."
  exit 1
fi

if ! command -v curl >/dev/null 2>&1 || ! command -v tar >/dev/null 2>&1; then
  apt-get update -y
  apt-get install -y curl tar
fi

REPO="Realynx/TransparentCloudProxy"
ARCHIVE_URL="$(curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" | grep browser_download_url | grep serverproxy-linux-x64.tar.gz | cut -d '"' -f 4 | head -n1)"

if [[ -z "${ARCHIVE_URL}" ]]; then
  echo "Could not find serverproxy-linux-x64 release asset."
  exit 1
fi

install_root="/opt/realynx-serverproxy"
bin_dir="${install_root}/bin"

mkdir -p "${bin_dir}"
curl -fL "${ARCHIVE_URL}" -o /tmp/serverproxy-linux-x64.tar.gz

tar -xzf /tmp/serverproxy-linux-x64.tar.gz -C "${bin_dir}"
chmod +x "${bin_dir}/TransparentCloudServerProxy.WebDashboard"

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

[Install]
WantedBy=multi-user.target
SERVICE

systemctl daemon-reload
systemctl enable --now realynx-serverproxy

echo "Installed and started realynx-serverproxy.service"
