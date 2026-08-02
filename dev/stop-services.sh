#!/usr/bin/env bash
# Stop all CoffeePeek services started by dev/run-services.sh.
set -uo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG_DIR="$ROOT/dev/logs"
for s in account shops moderation media gateway; do
  pid=$(cat "$LOG_DIR/$s.pid" 2>/dev/null || true)
  if [[ -n "${pid:-}" ]] && kill -0 "$pid" 2>/dev/null; then
    kill "$pid" 2>/dev/null && echo "stopped $s ($pid)"
  fi
  rm -f "$LOG_DIR/$s.pid"
done
