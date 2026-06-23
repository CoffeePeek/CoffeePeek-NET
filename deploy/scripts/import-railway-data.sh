#!/usr/bin/env bash
# Import PostgreSQL databases from Railway into local VPS Postgres (via docker compose).
# Fill RAILWAY_PG_* variables in deploy/.env before running.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DEPLOY_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${DEPLOY_DIR}"

if [[ ! -f .env ]]; then
  echo "Missing .env — copy from .env.example"
  exit 1
fi

set -a
# shellcheck disable=SC1091
source .env
set +a

for var in RAILWAY_PG_HOST RAILWAY_PG_PORT RAILWAY_PG_USER RAILWAY_PG_PASSWORD POSTGRES_USER POSTGRES_PASSWORD; do
  if [[ -z "${!var:-}" ]]; then
    echo "Set ${var} in .env"
    exit 1
  fi
done

if ! command -v docker >/dev/null 2>&1; then
  echo "docker is required"
  exit 1
fi

docker compose up -d postgres
until docker compose exec -T postgres pg_isready -U "${POSTGRES_USER}" >/dev/null 2>&1; do
  sleep 2
done

import_db() {
  local railway_db="$1"
  local local_db="$2"
  local dump_file="/tmp/coffeepeek-${local_db}.dump"

  echo "==> Importing ${railway_db} -> ${local_db}"
  docker run --rm \
    -e PGPASSWORD="${RAILWAY_PG_PASSWORD}" \
    public.ecr.aws/docker/library/postgres:17-alpine \
    pg_dump \
    -h "${RAILWAY_PG_HOST}" \
    -p "${RAILWAY_PG_PORT}" \
    -U "${RAILWAY_PG_USER}" \
    -d "${railway_db}" \
    -Fc > "${dump_file}"

  docker compose exec -T postgres psql -U "${POSTGRES_USER}" -d postgres -c "DROP DATABASE IF EXISTS ${local_db};"
  docker compose exec -T postgres psql -U "${POSTGRES_USER}" -d postgres -c "CREATE DATABASE ${local_db};"

  local container
  container="$(docker compose ps -q postgres)"
  docker cp "${dump_file}" "${container}:/tmp/restore.dump"
  docker compose exec -T postgres pg_restore \
    -U "${POSTGRES_USER}" \
    -d "${local_db}" \
    --no-owner \
    --no-privileges \
    /tmp/restore.dump
  docker compose exec -T postgres rm -f /tmp/restore.dump

  rm -f "${dump_file}"
}

import_db "cpaccountdb" "cpaccountdb"
import_db "cpshopsdb" "cpshopsdb"
import_db "cpmoderationdb" "cpmoderationdb"
import_db "cpmediadb" "cpmediadb"

echo "Railway PostgreSQL data imported. MinIO files must be copied separately if needed."
