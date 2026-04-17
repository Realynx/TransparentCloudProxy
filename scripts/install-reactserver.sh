#!/usr/bin/env bash
set -euo pipefail

install_root="/opt/realynx-reactserver"
archive_path="/tmp/reactserver"
service_name="realynx-reactserver"
service_file="/etc/systemd/system/${service_name}.service"

spinner_chars='|/-\\'
color_red=''
color_green=''
color_reset=''

if [[ -t 1 ]]; then
  color_red=$'\033[31m'
  color_green=$'\033[32m'
  color_reset=$'\033[0m'
fi

usage() {
  cat <<'USAGE'
Usage: install-reactserver.sh [--uninstall|-u]

  (no args)        Install/upgrade ReactServer
  --uninstall, -u  Stop and remove ReactServer service and files
USAGE
}

run_with_spinner() {
  local message="$1"
  shift

  if [[ ! -t 1 ]]; then
    "$@"
    return $?
  fi

  printf '%s ' "${message}"
  "$@" >/dev/null 2>&1 &
  local pid=$!
  local i=0

  while kill -0 "${pid}" >/dev/null 2>&1; do
    local char="${spinner_chars:i%${#spinner_chars}:1}"
    printf '\r%s [%s]' "${message}" "${char}"
    i=$((i + 1))
    sleep 0.1
  done

  wait "${pid}"
  local status=$?

  if [[ ${status} -eq 0 ]]; then
    printf '\r%s ${color_green}done${color_reset}\n' "${message}"
  else
    printf '\r%s ${color_red}failed${color_reset}\n' "${message}"
  fi

  return ${status}
}

read_interactive() {
  local prompt="$1"
  local response=""

  if [[ -r /dev/tty ]]; then
    printf '%s' "${prompt}" >/dev/tty
    IFS= read -r response </dev/tty
    printf '%s' "${response}"
    return 0
  fi

  return 1
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
        printf 'Please answer yes or no.\n' >/dev/tty
        ;;
    esac
  done
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
    run_with_spinner "Stopping service" systemctl disable --now "${service_name}" || true
  fi

  rm -f "${service_file}"

  if command -v systemctl >/dev/null 2>&1; then
    run_with_spinner "Reloading systemd" systemctl daemon-reload || true
    systemctl reset-failed "${service_name}" >/dev/null 2>&1 || true
  fi

  rm -rf "${install_root}"
  rm -f "${archive_path}.zip" "${archive_path}.tar.gz"

  echo "Uninstall complete."
  exit 0
fi

if ! command -v curl >/dev/null 2>&1 || ! command -v unzip >/dev/null 2>&1 || ! command -v tar >/dev/null 2>&1; then
  run_with_spinner "Updating apt package index" apt-get update -y
  run_with_spinner "Installing archive tools" apt-get install -y curl unzip tar
fi

if ! command -v node >/dev/null 2>&1 || ! command -v npm >/dev/null 2>&1; then
  run_with_spinner "Updating apt package index" apt-get update -y
  run_with_spinner "Installing Node.js and npm" apt-get install -y nodejs npm
fi

REPO="Realynx/TransparentCloudProxy"
release_json_file="$(mktemp /tmp/realynx-reactserver-release.XXXXXX.json)"
run_with_spinner "Fetching latest release metadata" curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" -o "${release_json_file}"
release_json="$(cat "${release_json_file}")"
rm -f "${release_json_file}"
ARCHIVE_URL="$(printf '%s' "${release_json}" | sed -n 's/.*"browser_download_url": "\([^"]*reactserver\.zip\)".*/\1/p' | head -n1)"

if [[ -z "${ARCHIVE_URL}" ]]; then
  ARCHIVE_URL="$(printf '%s' "${release_json}" | sed -n 's/.*"browser_download_url": "\([^"]*reactserver\.tar\.gz\)".*/\1/p' | head -n1)"
fi

if [[ -z "${ARCHIVE_URL}" ]]; then
  echo "Could not find reactserver release asset (.zip or .tar.gz)."
  exit 1
fi

mkdir -p "${install_root}"
rm -rf "${install_root:?}"/*

if [[ "${ARCHIVE_URL}" == *.zip ]]; then
  archive_path+=".zip"
  run_with_spinner "Downloading reactserver package" curl -fsSL "${ARCHIVE_URL}" -o "${archive_path}"
  run_with_spinner "Extracting package" unzip -oq "${archive_path}" -d "${install_root}"
else
  archive_path+=".tar.gz"
  run_with_spinner "Downloading reactserver package" curl -fsSL "${ARCHIVE_URL}" -o "${archive_path}"
  run_with_spinner "Extracting package" tar -xzf "${archive_path}" -C "${install_root}"
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
run_with_spinner "Installing production dependencies" npm ci --omit=dev

setup_service="$(prompt_yes_no "Setup a systemd service to run ReactServer at startup? [Y/n]: " "yes")"

if [[ "${setup_service}" == "yes" ]]; then
  cat >"${service_file}" <<'SERVICE'
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

  run_with_spinner "Reloading systemd" systemctl daemon-reload
  run_with_spinner "Starting service" systemctl enable --now "${service_name}"
else
  run_with_spinner "Starting ReactServer in background" bash -c "nohup /usr/bin/env npm --prefix '${install_root}' start >/dev/null 2>&1 &"
fi
