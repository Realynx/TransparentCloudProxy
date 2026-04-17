#!/usr/bin/env bash
set -euo pipefail

install_root="/opt/realynx-serverproxy"
bin_dir="${install_root}/bin"
archive_path="/tmp/serverproxy-linux-x64"
service_name="realynx-serverproxy"
service_file="/etc/systemd/system/${service_name}.service"

usage() {
  cat <<'USAGE'
Usage: install-serverproxy.sh [--uninstall|-u]

  (no args)        Install/upgrade ServerProxy
  --uninstall, -u  Stop and remove ServerProxy service and files
USAGE
}

read_interactive() {
  local prompt="$1"
  local response=""

  if [[ -t 0 ]]; then
    read -r -p "${prompt}" response
  elif [[ -r /dev/tty ]]; then
    read -r -p "${prompt}" response </dev/tty
  else
    return 1
  fi

  printf '%s' "${response}"
  return 0
}

prompt_yes_no() {
  local prompt="$1"
  local default="$2"
  local response=""

  while true; do
    if ! response="$(read_interactive "${prompt}")"; then
      printf '%s' "${default}"
      return 0
    fi

    case "${response}" in
      [yY]|[yY][eE][sS])
        printf 'yes'
        return 0
        ;;
      [nN]|[nN][oO])
        printf 'no'
        return 0
        ;;
      "")
        printf '%s' "${default}"
        return 0
        ;;
      *)
        echo "Please answer yes or no." >&2
        ;;
    esac
  done
}

print_startup_summary() {
  local logs=""
  local onekey_line=""
  local rootcred_line=""

  for _ in {1..20}; do
    logs="$(journalctl -u "${service_name}" -n 200 --no-pager 2>/dev/null || true)"
    onekey_line="$(printf '%s\n' "${logs}" | grep -F 'OneKey Pass:' | tail -n1 || true)"
    rootcred_line="$(printf '%s\n' "${logs}" | grep -F 'Root Cred:' | tail -n1 || true)"

    if [[ -n "${onekey_line}" || -n "${rootcred_line}" ]]; then
      break
    fi

    sleep 1
  done

  printf '\nStartup summary:\n\n'

  if [[ -n "${rootcred_line}" ]]; then
    printf '%s\n' "${rootcred_line}"
  fi

  if [[ -n "${onekey_line}" ]]; then
    printf '%s\n' "${onekey_line}"
  fi

  if [[ -z "${rootcred_line}" && -z "${onekey_line}" ]]; then
    echo "Could not find credential lines yet."
    echo "Run: journalctl -u ${service_name} --no-pager | grep -E 'Root Cred:|OneKey Pass:'"
  fi
}

if [[ "${EUID}" -ne 0 ]]; then
  echo "Please run as root (use sudo)."
  exit 1
fi

mode="install"

# Support both:
#   bash -s -- --uninstall   (normal stdin args)
#   bash -- --uninstall      (arg may become $0)
first_arg="${1:-}"
if [[ -z "${first_arg}" && "${0:-}" == "--uninstall" ]]; then
  first_arg="--uninstall"
elif [[ -z "${first_arg}" && "${0:-}" == "-u" ]]; then
  first_arg="-u"
elif [[ -z "${first_arg}" && "${0:-}" == "--help" ]]; then
  first_arg="--help"
elif [[ -z "${first_arg}" && "${0:-}" == "-h" ]]; then
  first_arg="-h"
fi

case "${first_arg}" in
  "")
    ;;
  --uninstall|-u)
    mode="uninstall"
    ;;
  --help|-h)
    usage
    exit 0
    ;;
  *)
    echo "Unknown argument: ${first_arg}"
    usage
    exit 1
    ;;
esac

if [[ "${mode}" == "uninstall" ]]; then
  if command -v systemctl >/dev/null 2>&1; then
    systemctl disable --now "${service_name}" >/dev/null 2>&1 || true
  fi

  rm -f "${service_file}"

  if command -v systemctl >/dev/null 2>&1; then
    systemctl daemon-reload || true
    systemctl reset-failed "${service_name}" >/dev/null 2>&1 || true
  fi

  rm -rf "${install_root}"
  rm -f "${archive_path}.zip" "${archive_path}.tar.gz"

  echo "Uninstalled ${service_name} and removed ${install_root}."
  exit 0
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

setup_service="$(prompt_yes_no "Setup a systemd service to run proxy at startup? [Y/n]: " "yes")"

if [[ "${setup_service}" == "yes" ]]; then
  cat >"${service_file}" <<'SERVICE'
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
  systemctl enable --now "${service_name}"

  echo "Installed and started ${service_name}.service"
  print_startup_summary
else
  echo "Install complete. Systemd service was not configured."
  echo "Run manually: ${bin_dir}/TransparentCloudServerProxy.WebDashboard"
fi
