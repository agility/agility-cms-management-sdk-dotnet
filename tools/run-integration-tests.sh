#!/usr/bin/env bash
# Runs the live integration tests against a test instance.
#
# Credentials come from the environment, or from an env file (default ~/.config/agility/sdk-test.env,
# override with AGILITY_TEST_ENV_FILE) that exports AGILITY_MGMT_TOKEN, AGILITY_INSTANCE_GUID and optionally
# AGILITY_LOCALE, AGILITY_MGMT_BASE_URL and AGILITY_ALLOW_WRITES=true. Keep that file chmod 600.
set -euo pipefail

env_file="${AGILITY_TEST_ENV_FILE:-$HOME/.config/agility/sdk-test.env}"
if [[ -z "${AGILITY_MGMT_TOKEN:-}" && -f "$env_file" ]]; then
  set -a
  # shellcheck disable=SC1090
  . "$env_file"
  set +a
fi

if [[ -z "${AGILITY_MGMT_TOKEN:-}" || -z "${AGILITY_INSTANCE_GUID:-}" ]]; then
  echo "Set AGILITY_MGMT_TOKEN and AGILITY_INSTANCE_GUID, or create $env_file." >&2
  exit 1
fi

cd "$(dirname "$0")/.."
echo "Running live tests (writes: ${AGILITY_ALLOW_WRITES:-false})"
exec dotnet test --project tests/Agility.Management.Sdk.IntegrationTests "$@"
