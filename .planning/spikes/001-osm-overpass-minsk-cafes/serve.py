#!/usr/bin/env python3
"""Serve the spike review queue on http://127.0.0.1:8765/"""

from __future__ import annotations

import json
import re
import urllib.error
import urllib.parse
import urllib.request
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

DIR = Path(__file__).resolve().parent
PORT = 8765
CACHE_PATH = DIR / "preview-cache.json"
OG_IMAGE = re.compile(r'<meta[^>]+property=["\']og:image["\'][^>]+content=["\']([^"\']+)["\']', re.I)
OG_IMAGE_REV = re.compile(r'<meta[^>]+content=["\']([^"\']+)["\'][^>]+property=["\']og:image["\']', re.I)
OG_TITLE = re.compile(r'<meta[^>]+property=["\']og:title["\'][^>]+content=["\']([^"\']+)["\']', re.I)
OG_DESC = re.compile(r'<meta[^>]+property=["\']og:description["\'][^>]+content=["\']([^"\']+)["\']', re.I)
TITLE = re.compile(r"<title[^>]*>([^<]+)</title>", re.I)

_cache: dict[str, dict] | None = None


def load_cache() -> dict[str, dict]:
    global _cache
    if _cache is None:
        if CACHE_PATH.exists():
            _cache = json.loads(CACHE_PATH.read_text(encoding="utf-8"))
        else:
            _cache = {}
    return _cache


def save_cache() -> None:
    CACHE_PATH.write_text(json.dumps(load_cache(), ensure_ascii=False, indent=2), encoding="utf-8")


def first_match(patterns: list[re.Pattern], html: str) -> str | None:
    for pat in patterns:
        m = pat.search(html)
        if m:
            return m.group(1).strip()
    return None


def fetch_preview(url: str) -> dict:
    cache = load_cache()
    if url in cache:
        return cache[url]
    parsed = urllib.parse.urlparse(url)
    if parsed.scheme not in {"http", "https"} or not parsed.netloc:
        return {"ok": False, "error": "bad url"}
    host = parsed.netloc.lower()
    if any(x in host for x in ("instagram.com", "yandex.", "facebook.com", "vk.com")):
        result = {"ok": False, "error": "no-embed"}
        cache[url] = result
        save_cache()
        return result
    req = urllib.request.Request(
        url,
        headers={"User-Agent": "CoffeePeekSpike/001 (local research preview)"},
        method="GET",
    )
    try:
        with urllib.request.urlopen(req, timeout=8) as resp:
            raw = resp.read(200_000)
            final = resp.geturl()
    except (urllib.error.URLError, TimeoutError, ValueError) as exc:
        result = {"ok": False, "error": str(exc)}
        cache[url] = result
        save_cache()
        return result
    html = raw.decode("utf-8", errors="ignore")
    image = first_match([OG_IMAGE, OG_IMAGE_REV], html)
    if image:
        image = urllib.parse.urljoin(final, image)
    result = {
        "ok": True,
        "image": image,
        "title": first_match([OG_TITLE, TITLE], html),
        "description": first_match([OG_DESC], html),
        "finalUrl": final,
    }
    cache[url] = result
    save_cache()
    return result


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(DIR), **kwargs)

    def do_GET(self):
        parsed = urllib.parse.urlparse(self.path)
        if parsed.path == "/preview":
            qs = urllib.parse.parse_qs(parsed.query)
            url = (qs.get("url") or [""])[0]
            body = json.dumps(fetch_preview(url), ensure_ascii=False).encode("utf-8")
            self.send_response(200)
            self.send_header("Content-Type", "application/json; charset=utf-8")
            self.send_header("Cache-Control", "no-store")
            self.end_headers()
            self.wfile.write(body)
            return
        return super().do_GET()


if __name__ == "__main__":
    httpd = ThreadingHTTPServer(("127.0.0.1", PORT), Handler)
    print(f"Review queue: http://127.0.0.1:{PORT}/", flush=True)
    httpd.serve_forever()
