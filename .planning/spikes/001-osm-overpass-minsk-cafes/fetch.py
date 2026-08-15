#!/usr/bin/env python3
"""Fetch Minsk coffee-related OSM objects via Overpass and normalize candidates."""

from __future__ import annotations

import json
import re
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timezone
from pathlib import Path

ENDPOINTS = [
    "https://overpass-api.de/api/interpreter",
    "https://overpass.kumi.systems/api/interpreter",
]

# Minsk city approx (south, west, north, east)
BBOX = (53.824, 27.389, 53.974, 27.761)

QUERY = f"""
[out:json][timeout:90];
(
  nwr[amenity=cafe]({BBOX[0]},{BBOX[1]},{BBOX[2]},{BBOX[3]});
  nwr[shop=coffee]({BBOX[0]},{BBOX[1]},{BBOX[2]},{BBOX[3]});
  nwr[amenity=vending_machine][vending=coffee]({BBOX[0]},{BBOX[1]},{BBOX[2]},{BBOX[3]});
);
out center tags;
"""

SPECIALTY_NAME = re.compile(
    r"specialty|спеш[еа]лти|third.?wave|thirdwave|обжар|roaster|brew bar|brewbar",
    re.I,
)
CHAIN_NAME = re.compile(
    r"starbucks|mcdonald|kfc|burger king|costa coffee|dunkin|шоколадница|"
    r"кофеин[аы]?$|coffeeshop company|gloria jean|costa\b",
    re.I,
)
VENDING_NAME = re.compile(
    r"автомат|vending|coffee.?point|кофе.?поинт|кофепоинт|"
    r"coffee machine|кофемат|кофейный аппарат",
    re.I,
)
CANTEEN_NAME = re.compile(
    r"столов|буфет|лидо|mcdonald|макдонал|kfc|burger king",
    re.I,
)
TOGO_CHAIN = re.compile(
    r"^varka\b|варка coffee|cofix|шоколадница|coffeeshop company|cinnabon|mccaf",
    re.I,
)


def fetch_overpass() -> dict:
    last_error: Exception | None = None
    for url in ENDPOINTS:
        form = urllib.parse.urlencode({"data": QUERY}).encode("utf-8")
        req = urllib.request.Request(
            url,
            data=form,
            method="POST",
            headers={"Content-Type": "application/x-www-form-urlencoded; charset=utf-8"},
        )
        try:
            with urllib.request.urlopen(req, timeout=120) as resp:
                return json.loads(resp.read().decode("utf-8"))
        except (urllib.error.URLError, TimeoutError, json.JSONDecodeError) as exc:
            last_error = exc
            continue
    raise RuntimeError(f"All Overpass endpoints failed: {last_error}")


def coords(el: dict) -> tuple[float | None, float | None]:
    if "lat" in el and "lon" in el:
        return el["lat"], el["lon"]
    center = el.get("center") or {}
    return center.get("lat"), center.get("lon")


def instagram_url(tags: dict, website: str | None) -> str | None:
    raw = tags.get("contact:instagram") or tags.get("instagram")
    if not raw and website and "instagram.com" in website.lower():
        raw = website
    if not raw:
        return None
    raw = raw.strip().replace("instgram.com", "instagram.com")
    if raw.startswith("http"):
        return raw
    return f"https://www.instagram.com/{raw.lstrip('@')}/"


def research_links(name: str, lat: float | None, lon: float | None, instagram: str | None, website: str | None) -> dict:
    q = urllib.parse.quote(f"{name} Минск")
    q_coffee = urllib.parse.quote(f"{name} Минск кофейня")
    ll = f"{lon},{lat}" if lat is not None and lon is not None else ""
    maps = f"https://yandex.by/maps/?text={q}&z=17"
    if ll:
        maps += f"&ll={ll}"
    return {
        "instagram": instagram,
        "instagramSearch": None if instagram else f"https://www.google.com/search?q={urllib.parse.quote(name + ' Минск instagram')}",
        "website": website,
        "yandexMaps": maps,
        "yandexImages": f"https://yandex.by/images/search?text={q_coffee}",
        "osm": None,
    }


def address(tags: dict) -> str | None:
    parts = [
        tags.get("addr:street"),
        tags.get("addr:housenumber"),
        tags.get("addr:city"),
    ]
    line = ", ".join(p for p in parts if p)
    return line or tags.get("addr:full")


def classify(tags: dict) -> tuple[str, list[str]]:
    reasons: list[str] = []
    amenity = tags.get("amenity")
    shop = tags.get("shop")
    cuisine = (tags.get("cuisine") or "").lower()
    name = tags.get("name") or tags.get("name:ru") or tags.get("name:en") or ""

    if amenity == "vending_machine" or tags.get("vending") == "coffee":
        reasons.append("osm:vending_machine")
        return "auto_reject", reasons
    if VENDING_NAME.search(name):
        reasons.append("name:vending-like")
        return "auto_reject", reasons
    if CANTEEN_NAME.search(name):
        reasons.append("name:canteen")
        return "likely_noise", reasons
    if TOGO_CHAIN.search(name) or (tags.get("brand") or "").lower() in {
        "varka",
        "cofix",
        "шоколадница",
        "starbucks",
        "mcdonald's",
        "kfc",
    }:
        reasons.append("name:to-go-chain")
        return "likely_noise", reasons
    if CHAIN_NAME.search(name):
        reasons.append("name:chain")
        return "likely_noise", reasons
    if SPECIALTY_NAME.search(name):
        reasons.append("name:specialty-signal")
        return "likely_specialty", reasons
    if shop == "coffee":
        reasons.append("osm:shop=coffee")
        return "priority", reasons
    if "coffee_shop" in cuisine or cuisine == "coffee":
        reasons.append(f"osm:cuisine={cuisine}")
        return "priority", reasons
    if "кофе" in name.lower() or "coffee" in name.lower():
        reasons.append("name:coffee")
        return "priority", reasons
    if amenity == "cafe":
        reasons.append("osm:amenity=cafe")
        return "review", reasons
    reasons.append("unknown")
    return "review", reasons


def normalize(raw: dict) -> dict:
    elements = raw.get("elements") or []
    seen: set[str] = set()
    candidates = []
    for el in elements:
        osm_type = el.get("type")
        osm_id = el.get("id")
        key = f"{osm_type}/{osm_id}"
        if key in seen:
            continue
        seen.add(key)
        tags = el.get("tags") or {}
        lat, lon = coords(el)
        bucket, reasons = classify(tags)
        name = tags.get("name") or tags.get("name:ru") or tags.get("name:en") or "(unnamed)"
        website = tags.get("website") or tags.get("contact:website")
        instagram = instagram_url(tags, website)
        links = research_links(name, lat, lon, instagram, website)
        links["osm"] = f"https://www.openstreetmap.org/{key}"
        candidates.append(
            {
                "source": "osm",
                "externalId": key,
                "name": name,
                "lat": lat,
                "lon": lon,
                "address": address(tags),
                "phone": tags.get("phone") or tags.get("contact:phone"),
                "website": website,
                "instagram": instagram,
                "facebook": tags.get("contact:facebook") or tags.get("facebook"),
                "vk": tags.get("contact:vk"),
                "description": tags.get("description") or tags.get("description:ru"),
                "openingHours": tags.get("opening_hours"),
                "amenity": tags.get("amenity"),
                "shop": tags.get("shop"),
                "cuisine": tags.get("cuisine"),
                "brand": tags.get("brand"),
                "operator": tags.get("operator"),
                "outdoorSeating": tags.get("outdoor_seating"),
                "indoorSeating": tags.get("indoor_seating"),
                "takeaway": tags.get("takeaway"),
                "internetAccess": tags.get("internet_access"),
                "links": links,
                "bucket": bucket,
                "signals": reasons,
                "tags": tags,
            }
        )

    counts: dict[str, int] = {}
    for c in candidates:
        counts[c["bucket"]] = counts.get(c["bucket"], 0) + 1

    return {
        "fetchedAtUtc": datetime.now(timezone.utc).isoformat(),
        "bbox": {"south": BBOX[0], "west": BBOX[1], "north": BBOX[2], "east": BBOX[3]},
        "query": QUERY.strip(),
        "total": len(candidates),
        "counts": counts,
        "candidates": candidates,
    }


def main() -> None:
    import sys

    out_dir = Path(__file__).resolve().parent
    raw_path = out_dir / "overpass-raw.json"
    if "--reuse-raw" in sys.argv and raw_path.exists():
        raw = json.loads(raw_path.read_text(encoding="utf-8"))
    else:
        raw = fetch_overpass()
        raw_path.write_text(json.dumps(raw, ensure_ascii=False), encoding="utf-8")
    payload = normalize(raw)
    (out_dir / "candidates.json").write_text(
        json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8"
    )
    ui = {
        "fetchedAtUtc": payload["fetchedAtUtc"],
        "total": payload["total"],
        "counts": payload["counts"],
        "candidates": [
            {k: c[k] for k in (
                "externalId", "name", "lat", "lon", "address", "phone",
                "website", "instagram", "facebook", "vk", "description",
                "openingHours", "amenity", "shop", "cuisine", "brand",
                "outdoorSeating", "indoorSeating", "takeaway", "internetAccess",
                "links", "bucket", "signals",
            )}
            for c in payload["candidates"]
        ],
    }
    (out_dir / "candidates-ui.json").write_text(
        json.dumps(ui, ensure_ascii=False), encoding="utf-8"
    )
    print(json.dumps({"total": payload["total"], "counts": payload["counts"]}, ensure_ascii=False))


if __name__ == "__main__":
    main()
