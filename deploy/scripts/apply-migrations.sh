#!/usr/bin/env bash
# Apply EF Core migrations to the VPS PostgreSQL instance.
# Requires .NET 10 SDK on the host (install: https://dot.net).
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
REPO_ROOT="$(cd "${DEPLOY_DIR}/.." && pwd)"

if [[ ! -f "${DEPLOY_DIR}/.env" ]]; then
  echo "Missing ${DEPLOY_DIR}/.env"
  exit 1
fi

set -a
# shellcheck disable=SC1091
source "${DEPLOY_DIR}/.env"
set +a

if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet SDK not found."
  exit 1
fi

if ! dotnet ef --version >/dev/null 2>&1; then
  dotnet tool install --global dotnet-ef
  export PATH="${PATH}:${HOME}/.dotnet/tools"
fi

export ASPNETCORE_ENVIRONMENT=Production

PG_HOST="${PG_HOST:-localhost}"
PG_PORT="${PG_PORT:-5432}"

apply_migration() {
  local name="$1"
  local context="$2"
  local project="$3"
  local startup="$4"
  local database="$5"
  local connection="Host=${PG_HOST};Port=${PG_PORT};Database=${database};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"

  echo "--- ${name} ---"
  export PostgresCpOptions__ConnectionString="${connection}"
  dotnet ef database update \
    --context "${context}" \
    --project "${REPO_ROOT}/${project}" \
    --startup-project "${REPO_ROOT}/${startup}" \
    --connection "${connection}" \
    --configuration Release
}

cd "${REPO_ROOT}"
dotnet restore CoffeePeek.Backend.ci.slnf -p:AllowMissingPrunePackageData=true
dotnet build CoffeePeek.Backend.ci.slnf --configuration Release --no-restore -m -p:AllowMissingPrunePackageData=true

apply_migration "Account" "AccountDbContext" \
  "CoffeePeek.Account.Persistence/CoffeePeek.Account.Persistence.csproj" \
  "CoffeePeek.AccountService/CoffeePeek.AccountService.csproj" \
  "cpaccountdb"

apply_migration "Shops" "ShopsDbContext" \
  "CoffeePeek.Shops.Persistance/CoffeePeek.Shops.Persistance.csproj" \
  "CoffeePeek.ShopsService/CoffeePeek.ShopsService.csproj" \
  "cpshopsdb"

apply_migration "Moderation" "ModerationDbContext" \
  "CoffeeShop.Moderation.Persistence/CoffeeShop.Moderation.Persistence.csproj" \
  "CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj" \
  "cpmoderationdb"

apply_migration "Media" "MediaDbContext" \
  "CoffeePeek.MediaService/CoffeePeek.MediaService.csproj" \
  "CoffeePeek.MediaService/CoffeePeek.MediaService.csproj" \
  "cpmediadb"

echo "All migrations applied."
