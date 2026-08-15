---
created: 2026-08-15T12:03:16+03:00
completed: 2026-08-15T12:07:00+03:00
title: Spike OSM Overpass Minsk cafe candidates
area: general
files:
  - .planning/spikes/001-osm-overpass-minsk-cafes/
---

## Problem

Нужно наполнить каталог specialty-кофеен Минска. Источник v1 — OSM (+ списки), не Yandex Places. Пока неизвестно, сколько кандидатов даёт Overpass и насколько они шумные; без этого нельзя спроектировать админ-очередь и автофильтр.

## Solution

Done in spike 001 (VALIDATED). Overpass bbox Минска → 1576 кандидатов. Очередь: http://127.0.0.1:8765/  Модель: `queue-model.md`.
