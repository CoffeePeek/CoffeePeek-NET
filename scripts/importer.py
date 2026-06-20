#!/usr/bin/env python3
"""
CoffeePeek Importer — Minsk coffee shops from OpenStreetMap

Usage:
  python importer.py fetch               # fetch from OSM → staging.json
  python importer.py review              # print staged shops
  python importer.py approve <osm_id>    # approve single shop
  python importer.py approve all         # approve all shops
  python importer.py import --db <dsn>   # load approved shops into PostgreSQL

Examples:
  python importer.py fetch
  python importer.py approve 123456789
  python importer.py approve all
  python importer.py import --db "host=localhost port=5432 dbname=shops user=postgres password=secret"

Dependencies:
  pip install requests psycopg2-binary
"""

import json
import sys
import uuid
import argparse
from datetime import datetime, timezone
from pathlib import Path

import requests

STAGING_FILE = Path(__file__).parent / "staging.json"

OVERPASS_URL = "https://overpass-api.de/api/interpreter"

# Minsk bounding box: south, west, north, east
MINSK_BBOX = "53.80,27.38,53.97,27.70"

OVERPASS_QUERY = f"""
[out:json][timeout:60];
(
  node["amenity"="cafe"]({MINSK_BBOX});
  way["amenity"="cafe"]({MINSK_BBOX});
  node["amenity"="coffee_shop"]({MINSK_BBOX});
  way["amenity"="coffee_shop"]({MINSK_BBOX});
);
out center tags;
"""

# Fixed system user ID used as CreatorId for all seeded shops
SEED_CREATOR_ID = "00000000-0000-0000-0000-000000000001"

# PriceRange enum: Cheap=1, Moderate=2, Expensive=3, Luxury=4
# CoffeeShopStatus enum: Active=0, TemporarilyClosed=1, PermanentlyClosed=2


def cmd_fetch(args):
    print("Querying OpenStreetMap Overpass API for Minsk cafes...")
    try:
        resp = requests.post(OVERPASS_URL, data={"data": OVERPASS_QUERY}, timeout=90)
        resp.raise_for_status()
    except requests.RequestException as e:
        print(f"Request failed: {e}")
        sys.exit(1)

    elements = resp.json().get("elements", [])
    shops = []

    for el in elements:
        tags = el.get("tags", {})

        name = tags.get("name") or tags.get("name:ru") or tags.get("name:en")
        if not name:
            continue

        if el["type"] == "node":
            lat, lon = el.get("lat"), el.get("lon")
        else:
            center = el.get("center", {})
            lat, lon = center.get("lat"), center.get("lon")

        if lat is None or lon is None:
            continue

        address = _build_address(tags)
        if not address:
            address = name  # fallback so NOT NULL constraint is satisfied

        shops.append({
            "approved": False,
            "osm_id": el["id"],
            "name": name[:100],
            "description": _truncate(tags.get("description") or tags.get("description:ru"), 500),
            "address": address[:500],
            "lat": lat,
            "lon": lon,
            "phone": _truncate(tags.get("phone") or tags.get("contact:phone"), 20),
            "email": _truncate(tags.get("email") or tags.get("contact:email"), 255),
            "website": _truncate(tags.get("website") or tags.get("contact:website"), 2048),
            "instagram": _truncate(_extract_instagram(tags), 255),
            "opening_hours": tags.get("opening_hours"),
            "price_range": _infer_price_range(tags),
        })

    staging = {
        "source": "osm",
        "city": "Minsk",
        "fetched_at": datetime.now(timezone.utc).isoformat(),
        "total": len(shops),
        "shops": shops,
    }

    STAGING_FILE.write_text(json.dumps(staging, ensure_ascii=False, indent=2), encoding="utf-8")
    print(f"Fetched {len(shops)} shops → {STAGING_FILE}")
    print('Edit "approved": false → true, then run: python importer.py import --db "..."')


def cmd_review(args):
    staging = _load_staging()
    shops = staging["shops"]
    approved = [s for s in shops if s["approved"]]
    pending = [s for s in shops if not s["approved"]]

    print(f"\n{'='*60}")
    print(f"Source : {staging['source']}  |  City: {staging['city']}")
    print(f"Fetched: {staging['fetched_at']}")
    print(f"Total  : {staging['total']}  |  Approved: {len(approved)}  |  Pending: {len(pending)}")
    print(f"{'='*60}")

    if approved:
        print(f"\n✓ APPROVED ({len(approved)})")
        for s in approved:
            print(f"  [{s['osm_id']}] {s['name']} — {s['address']}")

    if pending:
        print(f"\n· PENDING ({len(pending)})")
        for s in pending[:50]:
            phone = f"  {s['phone']}" if s["phone"] else ""
            print(f"  [{s['osm_id']}] {s['name']} — {s['address']}{phone}")
        if len(pending) > 50:
            print(f"  ... and {len(pending) - 50} more")

    print()


def cmd_approve(args):
    staging = _load_staging()
    shops = staging["shops"]

    if args.target == "all":
        count = 0
        for s in shops:
            if not s["approved"]:
                s["approved"] = True
                count += 1
        _save_staging(staging)
        print(f"Approved {count} shops.")
        return

    try:
        osm_id = int(args.target)
    except ValueError:
        print(f"Invalid OSM ID: {args.target}. Use a numeric ID or 'all'.")
        sys.exit(1)

    shop = next((s for s in shops if s["osm_id"] == osm_id), None)
    if not shop:
        print(f"Shop with osm_id={osm_id} not found in staging.")
        sys.exit(1)

    shop["approved"] = True
    _save_staging(staging)
    print(f"Approved: [{osm_id}] {shop['name']}")


def cmd_import(args):
    try:
        import psycopg2
    except ImportError:
        print("Install driver: pip install psycopg2-binary")
        sys.exit(1)

    staging = _load_staging()
    approved = [s for s in staging["shops"] if s["approved"]]

    if not approved:
        print('No approved shops. Run "approve" first or edit staging.json.')
        sys.exit(1)

    print(f"Connecting to DB...")
    try:
        conn = psycopg2.connect(args.db)
    except Exception as e:
        print(f"DB connection failed: {e}")
        sys.exit(1)

    cur = conn.cursor()
    now = datetime.now(timezone.utc)

    # Upsert city
    city_id = _upsert_city(cur, "Minsk", now)

    imported = 0
    skipped = 0

    for shop in approved:
        shop_id = str(uuid.uuid4())

        try:
            cur.execute(
                """
                INSERT INTO "Shops" (
                    "Id", "Name", "Description", "PriceRange", "Status",
                    "CreatorId", "ModerationId",
                    "InstagramLink", "Email", "SiteLink", "PhoneNumber",
                    "Address", "IsAddressValidated", "Latitude", "Longitude", "CityId",
                    "CreatedAtUtc", "UpdatedAtUtc"
                ) VALUES (
                    %s, %s, %s, %s, %s,
                    %s, %s,
                    %s, %s, %s, %s,
                    %s, %s, %s, %s, %s,
                    %s, %s
                )
                ON CONFLICT DO NOTHING
                """,
                (
                    shop_id,
                    shop["name"],
                    shop.get("description"),
                    shop.get("price_range", 2),  # Moderate by default
                    0,  # Active
                    SEED_CREATOR_ID,
                    None,  # ModerationId — no moderation record for seed data
                    shop.get("instagram"),
                    shop.get("email"),
                    shop.get("website"),
                    shop.get("phone"),
                    shop["address"],
                    True,
                    shop.get("lat"),
                    shop.get("lon"),
                    city_id,
                    now,
                    None,
                ),
            )
            imported += 1
        except Exception as e:
            print(f"  SKIP [{shop['osm_id']}] {shop['name']}: {e}")
            skipped += 1

    conn.commit()
    cur.close()
    conn.close()

    print(f"Done. Imported: {imported}, skipped: {skipped}.")
    print(f"City 'Minsk' id: {city_id}")


# ── helpers ──────────────────────────────────────────────────────────────────

def _upsert_city(cur, name: str, now: datetime) -> str:
    cur.execute('SELECT "Id" FROM "Cities" WHERE "Name" = %s LIMIT 1', (name,))
    row = cur.fetchone()
    if row:
        return row[0]

    city_id = str(uuid.uuid4())
    cur.execute(
        'INSERT INTO "Cities" ("Id", "Name", "CreatedAtUtc") VALUES (%s, %s, %s)',
        (city_id, name, now),
    )
    print(f"Created city '{name}' with id={city_id}")
    return city_id


def _build_address(tags: dict) -> str | None:
    street = tags.get("addr:street")
    house = tags.get("addr:housenumber")
    if street and house:
        return f"{street}, {house}"
    if street:
        return street
    return None


def _extract_instagram(tags: dict) -> str | None:
    raw = tags.get("contact:instagram") or tags.get("instagram")
    if not raw:
        return None
    return (
        raw.replace("https://www.instagram.com/", "")
           .replace("https://instagram.com/", "")
           .strip("/")
    )


def _infer_price_range(tags: dict) -> int:
    # OSM doesn't have a standard price_range tag, but some use it
    tag = tags.get("price_range") or tags.get("price")
    if tag:
        tag = tag.lower()
        if tag in ("$", "cheap", "1"):
            return 1
        if tag in ("$$$", "expensive", "3"):
            return 3
        if tag in ("$$$$", "luxury", "4"):
            return 4
    return 2  # Moderate


def _truncate(value: str | None, max_len: int) -> str | None:
    if value is None:
        return None
    return value[:max_len]


def _load_staging() -> dict:
    if not STAGING_FILE.exists():
        print(f"No staging file found. Run 'fetch' first.")
        sys.exit(1)
    return json.loads(STAGING_FILE.read_text(encoding="utf-8"))


def _save_staging(staging: dict):
    STAGING_FILE.write_text(json.dumps(staging, ensure_ascii=False, indent=2), encoding="utf-8")


# ── CLI ───────────────────────────────────────────────────────────────────────

def main():
    parser = argparse.ArgumentParser(description="CoffeePeek data importer for Minsk")
    sub = parser.add_subparsers(dest="command", required=True)

    sub.add_parser("fetch", help="Fetch coffee shops from OpenStreetMap")
    sub.add_parser("review", help="Print staged shops summary")

    p_approve = sub.add_parser("approve", help="Approve shops for import")
    p_approve.add_argument("target", help="OSM ID or 'all'")

    p_import = sub.add_parser("import", help="Import approved shops into PostgreSQL")
    p_import.add_argument(
        "--db",
        required=True,
        help='PostgreSQL DSN, e.g. "host=localhost port=5432 dbname=shops user=postgres password=secret"',
    )

    args = parser.parse_args()

    {
        "fetch": cmd_fetch,
        "review": cmd_review,
        "approve": cmd_approve,
        "import": cmd_import,
    }[args.command](args)


if __name__ == "__main__":
    main()
