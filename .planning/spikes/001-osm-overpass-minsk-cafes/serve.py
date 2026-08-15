#!/usr/bin/env python3
"""Serve the spike review queue on http://127.0.0.1:8765/"""

from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path

DIR = Path(__file__).resolve().parent
PORT = 8765


class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(DIR), **kwargs)


if __name__ == "__main__":
    httpd = ThreadingHTTPServer(("127.0.0.1", PORT), Handler)
    print(f"Review queue: http://127.0.0.1:{PORT}/")
    httpd.serve_forever()
