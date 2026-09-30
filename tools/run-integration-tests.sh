#!/usr/bin/env bash
# Runs the live integration tests against a test instance.
#
# Credentials come from the environment, or from an env file: AGILITY_TEST_ENV_FILE if set, else
# .env.integration in the repository root (git-ignored), else ~/.config/agility/sdk-test.env. The file
# exports AGILITY_MGMT_TOKEN, AGILITY_INSTANCE_GUID and optionally AGILITY_LOCALE, AGILITY_MGMT_BASE_URL
# and AGILITY_ALLOW_WRITES=true. Keep it chmod 600.
set -euo pipefail

repo_root="$(cd "$(dirname "$0")/.." && pwd)"
if [[ -n "${AGILITY_TEST_ENV_FILE:-}" ]]; then
  env_file="$AGILITY_TEST_ENV_FILE"
elif [[ -f "$repo_root/.env.integration" ]]; then
  env_file="$repo_root/.env.integration"
else
  env_file="$HOME/.config/agility/sdk-test.env"
fi
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

cd "$repo_root"
echo "Running live tests (writes: ${AGILITY_ALLOW_WRITES:-false})"
exec dotnet test --project tests/Agility.Management.Sdk.IntegrationTests "$@"
