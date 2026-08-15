---
created: 2026-08-15T12:03:16+03:00
title: Spike OSM Overpass Minsk cafe candidates
area: general
files:
  - CoffeePeek.Moderation.Infrastructure/External/Yandex/
  - CoffeePeek.Moderation.Domain/Aggregates/ModerationShopAggregate/
---

## Problem

Нужно наполнить каталог specialty-кофеен Минска. Источник v1 — OSM (+ списки), не Yandex Places. Пока неизвестно, сколько кандидатов даёт Overpass и насколько они шумные; без этого нельзя спроектировать админ-очередь и автофильтр.

## Solution

1. Overpass-запрос по bbox Минска: cafe / coffee_shop и смежные теги.
2. Выгрузить N кандидатов (имя, lat/lon, tags, osm id) в промежуточный формат.
3. Черновик модели очереди импорта в Moderation (поля кандидата + approve/reject) — без полной реализации UI.
4. Зафиксировать findings в `.planning/research/` или note; открыть SEED-001 когда готовы к фазе.
