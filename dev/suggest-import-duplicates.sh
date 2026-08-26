#!/usr/bin/env bash
# Scan import candidates for lookalike pairs (e.g. Belarusian vs Russian addresses)
# and queue them for admin confirm/reject. Does not merge anything.
#
# Requires a running gateway and a Moderator/Admin JWT:
#   ACCESS_TOKEN=... ./dev/suggest-import-duplicates.sh
set -euo pipefail

GW=${GATEWAY_URL:-http://localhost:8080}
TOKEN=${ACCESS_TOKEN:?set ACCESS_TOKEN to a Moderator or Admin JWT}

curl -sS -X POST "$GW/api/admin/import/duplicates/refresh" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -w "\nHTTP %{http_code}\n"
