#!/usr/bin/env bash
# End-to-end smoke test for the CoffeePeek dev environment.
# Exercises: Gateway (routing + JWT validation) -> Account + Shops -> Postgres.
# Requires infra + all services running (see dev/README steps in AGENTS.md).
set -euo pipefail

GW=${GATEWAY_URL:-http://localhost:8080}
PG_CONTAINER=${PG_CONTAINER:-coffeepeek-dev-infra-postgres-1}
EMAIL="barista+$(date +%s)@coffeepeek.dev"
USER="barista$(date +%s | tail -c 6)"
PASS="CoffeePeek123!"

line() { printf '\n\033[1m== %s ==\033[0m\n' "$1"; }

line "1. Register a new user  (POST $GW/api/Users)"
curl -sS -X POST "$GW/api/Users" -H 'Content-Type: application/json' \
  -d "{\"userName\":\"$USER\",\"email\":\"$EMAIL\",\"password\":\"$PASS\"}" -w "\nHTTP %{http_code}\n"

line "2. Confirm email  (token read from DB, simulating the email link)"
TOKEN=$(docker exec "$PG_CONTAINER" psql -U coffeepeek -d cpaccountdb -t -A \
  -c "SELECT \"EmailConfirmationToken\" FROM \"Users\" WHERE \"Email\"='$EMAIL';")
curl -sS -X PUT "$GW/api/Users/me/email-confirmation?token=$TOKEN" -w "\nHTTP %{http_code}\n"

line "3. Log in  (POST $GW/api/Tokens) -> JWT"
RESP=$(curl -sS -X POST "$GW/api/Tokens" -H 'Content-Type: application/json' \
  -d "{\"email\":\"$EMAIL\",\"password\":\"$PASS\"}")
ACCESS=$(echo "$RESP" | python3 -c "import sys,json;print(json.load(sys.stdin)['data']['accessToken'])")
echo "access token acquired (${#ACCESS} chars)"

line "4. Call authenticated endpoint  (GET $GW/api/Users/me, JWT validated at gateway)"
curl -sS "$GW/api/Users/me" -H "Authorization: Bearer $ACCESS" -w "\nHTTP %{http_code}\n"

line "5. Query Shops service via gateway  (GET $GW/api/CoffeeShops)"
curl -sS "$GW/api/CoffeeShops?page=1&pageSize=5" -H "Authorization: Bearer $ACCESS" -w "\nHTTP %{http_code}\n"

line "DONE - environment verified end to end"
