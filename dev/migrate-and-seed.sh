#!/usr/bin/env bash
# Apply EF Core migrations to the 4 CoffeePeek databases and seed the Roles table.
# Requires the dev infra to be running (dev/docker-compose.dev.yml) and dotnet-ef installed:
#   dotnet tool install --global dotnet-ef --version "10.*"
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

export DOTNET_ROOT=${DOTNET_ROOT:-/usr/share/dotnet}
export PATH="$PATH:$DOTNET_ROOT:$HOME/.dotnet/tools"

# shellcheck disable=SC1091
set -a; source dev/dev.env; set +a

PG_CONTAINER=${PG_CONTAINER:-coffeepeek-dev-infra-postgres-1}

mig() {
  local db="$1" proj="$2" startup="$3" ctx="$4"
  echo "== migrate $ctx -> $db =="
  PostgresCpOptions__ConnectionString="Host=localhost;Port=5432;Database=$db;Username=coffeepeek;Password=coffeepeek;SslMode=Disable" \
    dotnet ef database update --project "$proj" --startup-project "$startup" --context "$ctx"
}

mig cpaccountdb    CoffeePeek.Account.Persistence      CoffeePeek.AccountService     AccountDbContext
mig cpshopsdb      CoffeePeek.Shops.Persistance        CoffeePeek.ShopsService       ShopsDbContext
mig cpmoderationdb CoffeeShop.Moderation.Persistence   CoffeePeek.ModerationService  ModerationDbContext
mig cpmediadb      CoffeePeek.MediaService             CoffeePeek.MediaService       MediaDbContext

echo "== seed roles (cpaccountdb) =="
docker exec "$PG_CONTAINER" psql -U coffeepeek -d cpaccountdb -c \
  "INSERT INTO \"Roles\" (\"Id\", \"Name\", \"CreatedAtUtc\") \
   SELECT gen_random_uuid(), r, now() FROM (VALUES ('User'),('Admin'),('Moderator'),('Owner'),('Employee'),('Roaster')) AS t(r) \
   WHERE NOT EXISTS (SELECT 1 FROM \"Roles\" WHERE \"Name\" = t.r);"

echo "Done."
