#!/usr/bin/env bash
set -euo pipefail

install_root="/opt/realynx-serverproxy"
bin_dir="${install_root}/bin"
archive_path="/tmp/serverproxy-linux-x64"
service_name="realynx-serverproxy"
service_file="/etc/systemd/system/${service_name}.service"
server_binary_path=""

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
Usage: install-serverproxy.sh [--uninstall|-u]

  (no args)        Install/upgrade ServerProxy
  --uninstall, -u  Stop and remove ServerProxy service and files
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
    printf "\r%s ${color_green}done${color_reset}\n" "${message}"
  else
    printf "\r%s ${color_red}failed${color_reset}\n" "${message}"
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

detect_server_binary() {
  local candidates=(
    "TransparentCloudServerProxy.Server"
    "TransparentCloudServerProxy.WebDashboard"
  )

  for candidate in "${candidates[@]}"; do
    if [[ -f "${bin_dir}/${candidate}" ]]; then
      server_binary_path="${bin_dir}/${candidate}"
      return 0
    fi
  done

  local discovered
  discovered="$(find "${bin_dir}" -maxdepth 1 -type f -name 'TransparentCloudServerProxy*' \
    ! -name '*.dll' \
    ! -name '*.deps.json' \
    ! -name '*.runtimeconfig.json' \
    ! -name '*.pdb' | head -n1 || true)"

  if [[ -n "${discovered}" ]]; then
    server_binary_path="${discovered}"
    return 0
  fi

  return 1
}

extract_root_cred() {
  local text="$1"
  printf '%s\n' "${text}" | sed -n 's/.*Root Cred:[[:space:]]*\([^[:space:]]\+\).*/\1/p' | tail -n1
}

extract_onekey() {
  local text="$1"
  printf '%s\n' "${text}" | sed -n 's/.*OneKey Pass:[[:space:]]*\([^[:space:]]\+\).*/\1/p' | tail -n1
}

print_startup_summary_from_service() {
  local logs=""
  local root_cred=""
  local onekey=""

  if [[ -t 1 ]]; then
    printf 'Waiting for startup credentials '
  fi

  for i in {1..30}; do
    logs="$(journalctl -u "${service_name}" -n 300 --no-pager 2>/dev/null || true)"
    root_cred="$(extract_root_cred "${logs}")"
    onekey="$(extract_onekey "${logs}")"

    if [[ -n "${root_cred}" || -n "${onekey}" ]]; then
      break
    fi

    if [[ -t 1 ]]; then
      local char="${spinner_chars:i%${#spinner_chars}:1}"
      printf '\rWaiting for startup credentials [%s]' "${char}"
    fi

    sleep 1
  done

  if [[ -t 1 ]]; then
    printf '\r%-60s\r' ''
  fi

  if [[ -n "${root_cred}" ]]; then
    echo "RootCredential: ${root_cred}"
  fi

  if [[ -n "${onekey}" ]]; then
    echo "OneKey: ${color_red}${onekey}${color_reset}"
  fi

  if [[ -z "${root_cred}" && -z "${onekey}" ]]; then
    echo "Failed to detect RootCredential/OneKey from service logs."
    echo "Run: journalctl -u ${service_name} --no-pager | grep -E 'Root Cred:|OneKey Pass:'"
  fi
}

run_manual_and_print_credentials_only() {
  local manual_log
  local server_pid=""
  local root_cred=""
  local onekey=""

  manual_log="$(mktemp /tmp/realynx-serverproxy-manual.XXXXXX.log)"

  "${server_binary_path}" >"${manual_log}" 2>&1 &
  server_pid="$!"

  cleanup_manual() {
    if [[ -n "${server_pid}" ]] && kill -0 "${server_pid}" >/dev/null 2>&1; then
      kill "${server_pid}" >/dev/null 2>&1 || true
      wait "${server_pid}" >/dev/null 2>&1 || true
    fi
    rm -f "${manual_log}"
  }
  trap cleanup_manual EXIT INT TERM

  if [[ -t 1 ]]; then
    printf 'Waiting for startup credentials '
  fi

  for i in {1..30}; do
    if [[ -s "${manual_log}" ]]; then
      root_cred="$(extract_root_cred "$(cat "${manual_log}")")"
      onekey="$(extract_onekey "$(cat "${manual_log}")")"
    fi

    if [[ -n "${root_cred}" || -n "${onekey}" ]]; then
      break
    fi

    if ! kill -0 "${server_pid}" >/dev/null 2>&1; then
      break
    fi

    if [[ -t 1 ]]; then
      local char="${spinner_chars:i%${#spinner_chars}:1}"
      printf '\rWaiting for startup credentials [%s]' "${char}"
    fi

    sleep 1
  done

  if [[ -t 1 ]]; then
    printf '\r%-60s\r' ''
  fi

  if [[ -n "${root_cred}" ]]; then
    echo "RootCredential: ${root_cred}"
  fi

  if [[ -n "${onekey}" ]]; then
    echo "OneKey: ${color_red}${onekey}${color_reset}"
  fi

  if [[ -z "${root_cred}" && -z "${onekey}" ]]; then
    echo "Failed to detect RootCredential/OneKey from manual startup logs."
  fi

  if kill -0 "${server_pid}" >/dev/null 2>&1; then
    wait "${server_pid}" || true
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

if ! command -v curl >/dev/null 2>&1 || ! command -v tar >/dev/null 2>&1 || ! command -v unzip >/dev/null 2>&1; then
  run_with_spinner "Updating apt package index" apt-get update -y
  run_with_spinner "Installing required packages" apt-get install -y curl tar unzip
fi

REPO="Realynx/TransparentCloudProxy"
release_json_file="$(mktemp /tmp/realynx-serverproxy-release.XXXXXX.json)"
run_with_spinner "Fetching latest release metadata" curl -fsSL "https://api.github.com/repos/${REPO}/releases/latest" -o "${release_json_file}"
release_json="$(cat "${release_json_file}")"
rm -f "${release_json_file}"
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
  run_with_spinner "Downloading serverproxy package" curl -fsSL "${ARCHIVE_URL}" -o "${archive_path}"
  run_with_spinner "Extracting package" unzip -oq "${archive_path}" -d "${bin_dir}"
else
  archive_path+=".tar.gz"
  run_with_spinner "Downloading serverproxy package" curl -fsSL "${ARCHIVE_URL}" -o "${archive_path}"
  run_with_spinner "Extracting package" tar -xzf "${archive_path}" -C "${bin_dir}"
fi

if ! detect_server_binary; then
  echo "Could not find published server executable in ${bin_dir}."
  echo "Files found:"
  ls -la "${bin_dir}" || true
  exit 1
fi

chmod +x "${server_binary_path}"

setup_service="$(prompt_yes_no "Setup a systemd service to run proxy at startup? [Y/n]: " "yes")"

if [[ "${setup_service}" == "yes" ]]; then
  cat >"${service_file}" <<SERVICE
[Unit]
Description=Realynx ServerProxy
After=network.target

[Service]
Type=simple
WorkingDirectory=/opt/realynx-serverproxy/bin
ExecStart=${server_binary_path}
Restart=always
RestartSec=5
Environment=ASPNETCORE_URLS=http://0.0.0.0:8080
Environment=Kestrel__Endpoints__Http__Url=http://0.0.0.0:8080
Environment=Kestrel__Endpoints__Https__Url=http://127.0.0.1:0

[Install]
WantedBy=multi-user.target
SERVICE

  run_with_spinner "Reloading systemd" systemctl daemon-reload
  run_with_spinner "Starting service" systemctl enable --now "${service_name}"

  print_startup_summary_from_service
else
  run_manual_and_print_credentials_only
fi
