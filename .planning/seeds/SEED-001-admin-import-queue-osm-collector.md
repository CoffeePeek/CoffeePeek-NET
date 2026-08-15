---
id: SEED-001
status: dormant
planted: 2026-08-15
planted_during: tech-debt / post Tier-0 catalog bootstrap
trigger_when: when starting catalog content bootstrap or a "shops data / import" milestone
scope: medium — Moderation admin queue + OSM collector + approve→Shops path
---

# SEED-001: Admin import queue + OSM collector

## Why This Matters

Без кофеен в БД CoffeePeek пустой. Нужен контролируемый bootstrap: широкий сбор кандидатов (OSM + списки) → админ-очередь с категорией Kind → в ленту все одобренные (не только specialty). Ручной этап на малой выборке, затем автофильтр.

## When to Surface

**Trigger:** when starting catalog content bootstrap or a "shops data / import" milestone

Поверхность при `/gsd:new-milestone`, если в scope есть наполнение шопов, импорт, OSM, админ-модерация кандидатов.

## Scope Estimate

**Medium** — новая (или расширенная) сущность кандидатов в Moderation, админ API/UI-контракты, OSM Overpass collector, approve → создание `CoffeeShop`, позже scoring/автофильтр. Без хранения Yandex Places org payloads.

## Breadcrumbs

- `.planning/notes/2026-08-15-specialty-coffee-shops-minsk-import.md`
- `CoffeePeek.Moderation.*` — очередь модерации шопов
- `CoffeePeek.Moderation.Infrastructure/External/Yandex/` — только geocode
- `CoffeePeek.Shops.Domain` — `CoffeeShop` (прод после approve)
- Research: Yandex Places требует storage license; MVP = OSM + lists

## Not in this seed

- Импорт зёрен / tasting notes
- HTML-scrape Яндекс.Карт
- Persist Yandex Organization Search results without proper license
