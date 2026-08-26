# SPEC: Coffee shop menu (catalog drinks + photo parse)

**Status:** ready to implement  
**Audience:** backend agent (this repo) + client/admin-UI (separate)  
**Created:** 2026-08-26  
**Parser:** Gemini vision (`generateContent` + JSON schema) — locked

---

## Goal

У каждой кофейни есть **меню кофейных напитков** из общего каталога: напиток либо есть (с ценой), либо нет, либо ещё неизвестен. Модератор (очередь каталога + заявка пользователя) и пользователь при создании кофейни загружают **фото меню**. Gemini распознаёт позиции, маппит на каталог, считает `priceRange` по среднему чеку в BYN. Клиент в профиле кофейни видит меню и дату, когда оно было загружено/обновлено. Данные можно править руками.

## Background (код сегодня)

- Меню **нет**. `CoffeeShopDetailsDto` отдаёт зёрна, методы, оборудование, `priceRange` — не прайс напитков.
- `Beverages` в Shops Domain объявлен и **нигде не используется** — не брать как основу.
- `PriceRange` уже есть: `Cheap | Moderate | Expensive`. Сейчас задаётся руками, не из цен.
- Две очереди модерации (не смешивать сущности):
  - `ShopImportCandidate` — каталог (OSM / CoffeeMap / file). `/api/admin/import/...`
  - `ModerationShop` — заявка пользователя. `POST /api/ModerationShops` уже принимает `ShopPhotos`.
- Фото: Media `POST /api/Photos/shop` (presign) → confirm. Галерея кофейни ≠ фото меню.
- Публикация импорта: `ImportCandidatePublishedEvent` → `CreateShopFromImportService` (меню в событии нет).
- Approve заявки: `CoffeeShopApprovedEvent` → `CreateShopFromModerationService`.

**Продуктовый язык:** очередь каталога — это **модерация**, OSM только один из источников. HTTP `/api/admin/import` в этой фазе **не переименовываем** (ломает клиентов). UI копирайт: «Модерация». Ребрендинг роутов — отдельный follow-up.

---

## Locked decisions

1. **Меню = кофейные напитки**, не еда, не десерты, не мерч.
2. **Каталог глобальный** (seed). У кофейни — наличие/цена по каждому slug, не свободный список в v1.
3. **Custom/авторские** — схема заложена, в публичный ответ v1 **не отдаём**. Сырой unmatched с парсера храним.
4. **Источник правды после публикации** — Shops (`ShopMenu` на `CoffeeShop`). До публикации — черновик на кандидате / `ModerationShop`.
5. **Парсер** — Gemini multimodal `generateContent`, `response_mime_type=application/json` + schema. Байты картинки из MinIO (`inlineData`). Не Yandex Vision, не Tesseract.
6. **Ценовой диапазон** из среднего арифметического **известных цен Present-позиций** в BYN:
   - `< 7.00` → `Cheap`
   - `7.00 … 9.00` включительно → `Moderate` («около 8»)
   - `> 9.00` → `Expensive`  
   Пороги в `MenuPriceRangeOptions`, не хардкод в хендлерах. Если цен нет — `priceRange` не трогаем.
7. **Дата для клиента** — `capturedAtUtc` (когда загрузили/привязали фото, с которых собрали меню), плюс `updatedAtUtc` (последняя правка человеком или повторный парсинг).
8. **Редактирование v1:** moderator + admin. Автор заявки может только **приложить фото при создании**. После публикации — `PUT /api/admin/shops/{id}/menu`.
9. **Поиск/карта** полное меню **не несут**. Только деталка кофейни.
10. Парсинг **асинхронный** (Wolverine). HTTP не ждёт Gemini.

---

## Catalog v1 (seed, slug immutable)

| slug | nameRu | nameEn | category | aliases (parser) |
|------|--------|--------|----------|------------------|
| espresso | Эспрессо | Espresso | espresso | эспрессо, espresso |
| doppio | Доппио | Doppio | espresso | доппио, doppio, double espresso, двойной эспрессо |
| americano | Американо | Americano | espresso | американо, americano, lungo |
| cappuccino | Капучино | Cappuccino | espresso | капучино, капуч, cappuccino, cappucino |
| latte | Латте | Latte | espresso | латте, latte, caffe latte |
| flat_white | Флэт уайт | Flat white | espresso | флэт уайт, флэт, flat white, flatwhite |
| cortado | Кортадо | Cortado | espresso | кортадо, cortado, piccolo, пикколо |
| macchiato | Макиато | Macchiato | espresso | макиато, macchiato |
| raf | Раф | Raf | espresso | раф, raf |
| batch_brew | Фильтр | Batch brew | filter | фильтр, капелька, batch brew, drip, batch |
| v60 | V60 / воронка | V60 | filter | v60, v-60, воронка, hario, pour over |
| kalita | Калита | Kalita | filter | калита, kalita, wave |
| chemex | Кемекс | Chemex | filter | кемекс, chemex |
| aeropress | Аэропресс | AeroPress | filter | аэропресс, aeropress, aero press |

Новые стандартные напитки = **миграция seed**, не произвольный insert из UI в v1.

`kind` на определении: `Standard` сейчас; `Custom` зарезервирован.

---

## Domain

### Shops (published)

`CoffeeDrinkDefinition` — справочник (Id, Slug unique, names, category, aliases[], sortOrder, kind, isActive).

`ShopMenu` (1:1 с `CoffeeShop`):

- `CapturedAtUtc` — дата загрузки исходных фото (клиентская «актуальность»)
- `UpdatedAtUtc`, `UpdatedByUserId?`
- `Currency` = `BYN`
- `ParseStatus`: `None | Pending | Running | Ready | Failed`
- `ParseError?`
- `SuggestedPriceRange?` — посчитанный из позиций; при Apply пишется в `CoffeeShop.PriceRange`

`ShopMenuItem`:

- `DrinkDefinitionId`
- `Availability`: `Unknown | Present | Absent`
- `Price?` (decimal, Present)
- `VolumeMl?` (nullable, v1 можно не заполнять)
- `Source`: `Parsed | Manual`
- unique `(ShopMenuId, DrinkDefinitionId)`

`ShopMenuPhoto` — storageKey + media photo id; **не** в `ShopPhotos` галереи.

Future (колонки сразу, публично не маппить):

- `ShopMenuItem.Kind` (`Standard | Custom`)
- `CustomName?` когда Kind=Custom
- `ShopMenu.UnmatchedJson` — сырьё парсера `{ rawName, price, confidence }`

### Moderation (draft)

На `ShopImportCandidate` и `ModerationShop` — owned/jsonb `MenuDraft` той же формы, что публичный DTO (items по всем slug каталога + photos + parseStatus + capturedAtUtc). Не плодить вторую реляционную модель до publish.

При `decide Published` / approve заявки черновик копируется в `ShopMenu`. Если парсинг ещё `Pending/Running` — публиковать **можно**, меню доедет повторным событием `MenuDraftReady` **или** модератор ждёт Ready. **Locked:** публиковать можно с неполным меню; после Ready, если shop уже создан, Shops применяет черновик по `ResultingShopId` / `ModerationId`.

---

## Parse pipeline

1. Клиент получает presign `POST /api/Photos/menu`, грузит в MinIO, confirm.
2. Модератор/пользователь привязывает `UploadedPhotoDto[]` к кандидату / заявке / опубликованной кофейне.
3. Command кладёт `ParseStatus=Pending`, публикует Wolverine `ParseShopMenuCommand`.
4. Handler: читает байты из MinIO, шлёт в Gemini (до N фото за запрос, v1 max **4**), JSON:

```json
{
  "drinks": [
    { "rawName": "Капучино", "price": 9.0, "volumeMl": null, "confidence": 0.86 }
  ],
  "unmatched": [],
  "currencyGuess": "BYN"
}
```

5. Маппинг: aliases каталога (case-insensitive, RU/EN). Несколько цен на один slug → берём **минимальную** (обычно меньший объём). Несматченное → `unmatched`, не создаём Custom в v1.
6. Для каждого slug каталога: матч → `Present`+price; нет в фото → **не ставим Absent**, оставляем `Unknown` (отсутствие на фото ≠ «не готовят»). Absent только руками.
7. Считаем suggested `priceRange`. Модератор видит и может перезаписать.
8. `Failed`: статус + `parseError`, черновик не затираем.

**Gemini:** `GeminiOptions: ApiKey, Model=gemini-2.5-flash, TimeoutSeconds=60`. HTTP `generativelanguage.googleapis.com` + `IHttpClientFactory`. Ключ — env/user-secrets, не в git.

---

## Contract (JSON, enums как строки)

### `CoffeeDrinkCategory`

`espresso` | `filter`

### `MenuItemAvailability`

`Unknown` | `Present` | `Absent`

### `MenuParseStatus`

`None` | `Pending` | `Running` | `Ready` | `Failed`

### `CoffeeDrinkDefinitionDto`

```json
{
  "slug": "cappuccino",
  "nameRu": "Капучино",
  "nameEn": "Cappuccino",
  "category": "espresso",
  "sortOrder": 40
}
```

### `ShopMenuItemDto`

```json
{
  "slug": "cappuccino",
  "nameRu": "Капучино",
  "nameEn": "Cappuccino",
  "category": "espresso",
  "availability": "Present",
  "price": 9.00,
  "currency": "BYN",
  "volumeMl": null,
  "source": "Parsed"
}
```

Публичный профиль: **все** slug каталога (клиент сам прячет `Unknown`). Custom не включаем.

### `ShopMenuDto`

```json
{
  "capturedAtUtc": "2026-08-26T18:00:00Z",
  "updatedAtUtc": "2026-08-26T18:12:00Z",
  "currency": "BYN",
  "parseStatus": "Ready",
  "parseError": null,
  "suggestedPriceRange": "Moderate",
  "items": [ "...ShopMenuItemDto" ],
  "photos": [ { "id": "uuid", "fullUrl": "https://..." } ]
}
```

`null` меню на кофейне = ещё не заводили (клиент: пустое состояние, не 404).

### Additive на существующих DTO

- `CoffeeShopDetailsDto.Menu` → `ShopMenuDto?`
- `ModerationShopDto.Menu` → `ShopMenuDto?`
- `ShopImportCandidateDto.Menu` → `ShopMenuDto?`
- `SendCoffeeShopToModerationCommand.MenuPhotos` → `List<UploadedPhotoDto>?` (как `ShopPhotos`)
- `ImportCandidatePublishedItem` + payload меню (items snapshot) — **additive**, не ломает старых консьюмеров, если поле optional.

---

## Endpoints

Все новые поля additive. Gateway: существующие кластера + при необходимости путь `/api/menu/drinks` → shops-cluster.

### Public

| Method | Path | Auth | Назначение |
|--------|------|------|------------|
| GET | `/api/menu/drinks` | anonymous | Справочник стандартных напитков (кэш) |
| GET | `/api/CoffeeShops/{id}` | anonymous | Уже есть; в `data` добавляется `menu` |

### Media

| Method | Path | Auth | Назначение |
|--------|------|------|------------|
| POST | `/api/Photos/menu` | user | Presign, как shop, bucket Shop, prefix `menus/`. Не попадает в галерею |

### Пользователь — заявка на кофейню

| Method | Path | Auth | Назначение |
|--------|------|------|------------|
| POST | `/api/ModerationShops` | user | Как сейчас + optional `menuPhotos[]`. Ставит parse Pending |

Отдельный «мой черновик меню» пользователю в v1 не делаем — правки после сабмита только модератор.

### Модерация каталога (import candidate)

| Method | Path | Auth | Назначение |
|--------|------|------|------------|
| GET | `/api/admin/import/candidates/{id}` | moderator | Уже есть; `menu` в dossier |
| POST | `/api/admin/import/candidates/{id}/menu/photos` | moderator | Body: `{ photos: UploadedPhotoDto[] }`. Привязка + enqueue parse. Ставит `capturedAtUtc` если пусто |
| POST | `/api/admin/import/candidates/{id}/menu/parse` | moderator | Повторный прогон по уже привязанным фото |
| PUT | `/api/admin/import/candidates/{id}/menu` | moderator | Ручная правка items (`availability`, `price`). `source=Manual` на изменённых. Можно проставить Absent. Optional `applySuggestedPriceRange: true` |

### Модерация заявок пользователя

| Method | Path | Auth | Назначение |
|--------|------|------|------------|
| GET | `/api/ModerationShops/{id}` | moderator | + `menu` |
| POST | `/api/ModerationShops/{id}/menu/photos` | moderator | как у candidate |
| POST | `/api/ModerationShops/{id}/menu/parse` | moderator | повтор |
| PUT | `/api/ModerationShops/{id}/menu` | moderator | ручная правка |

### Опубликованная кофейня (admin)

| Method | Path | Auth | Назначение |
|--------|------|------|------------|
| GET | `/api/admin/shops/{id}/menu` | admin | полное меню + unmatched (для отладки) |
| POST | `/api/admin/shops/{id}/menu/photos` | admin | фото + parse |
| POST | `/api/admin/shops/{id}/menu/parse` | admin | повтор |
| PUT | `/api/admin/shops/{id}/menu` | admin | правка + optional apply priceRange на `CoffeeShop` |

---

## Flows

```
User/Moderator → Photos/menu presign → MinIO → confirm
     → attach photos (candidate | moderation shop | published shop)
     → ParseShopMenuCommand (Wolverine)
     → Gemini → map catalog → draft/shop menu Ready
     → moderator PUT corrections
     → publish/approve copies menu to CoffeeShop
     → client GET CoffeeShops/{id} видит menu + capturedAtUtc
```

`priceRange` на карточке: после Ready предлагаем; в публичный `CoffeeShop.PriceRange` пишем если модератор/admin подтвердил (`applySuggestedPriceRange`) **или** при publish, если у шопа ещё дефолт и suggested есть. **Locked:** при publish из импорта, если `CoffeeShop.PriceRange` не задан явно — берём suggested. Если модератор уже выставил диапазон руками — не перезаписывать.

---

## Tests (минимум)

- Seed: все v1 slug уникальны, aliases маппят «воронка» → `v60`, «доппио» → `doppio`.
- Parse mapper: два названия одного slug → одна позиция, min price.
- Unmatched не попадает в public `ShopMenuDto`.
- Unknown не становится Absent без PUT.
- PriceRange: среднее 6.5 → Cheap, 8 → Moderate, 10 → Expensive; пустой набор цен → null suggested.
- GET details без меню → `menu: null`.
- GET details с меню → все catalog slugs присутствуют.
- Publish candidate копирует Present/Absent/цены и `capturedAtUtc`.
- `SendCoffeeShopToModeration` с `menuPhotos` создаёт draft Pending (parser мокается).
- Gemini fail → Failed + прежний draft жив.
- Меню-фото не сериализуются в `CoffeeShopDetailsDto.Photos`.

---

## Boundaries

**In scope**

- Каталог + публичное меню в деталке
- Фото + Gemini parse
- Черновик в обеих очередях модерации
- Ручной PUT
- Авто-`priceRange` из среднего
- `capturedAtUtc` / `updatedAtUtc`
- Задел Custom (колонки + unmatched)

**Out of scope**

- Еда, десерты, авторские в публичном API
- Несколько размеров как отдельные SKU (только min price)
- Скрейп Instagram/сайта
- Переименование `/api/admin/import` → `/api/admin/moderation`
- Меню в search/map DTO
- Пользователь редактирует меню после сабмита / owner self-serve
- История ревизий меню (есть только две даты)
- Мультивалютность

---

## Implementation waves

1. **Contract + catalog + ShopMenu schema** (Shops migration, seed, GET `/api/menu/drinks`, `menu: null` в details).
2. **Gemini parser + Wolverine job** (adapter, mapper, options, тесты маппинга без живого API).
3. **Import candidate** photos/parse/PUT + поле в dossier DTO.
4. **User suggestion** `menuPhotos` + те же admin endpoints на `ModerationShops`.
5. **Publish/approve copy** + apply suggested priceRange.
6. **Admin published shop** GET/PUT/photos/parse.

Каждая волна — собираемый срез с тестами, не «сначала вся инфраструктура».

---

## Acceptance

- [ ] Справочник отдаёт все v1 slug
- [ ] Модератор каталога может приложить фото меню, статус проходит Pending → Ready (или Failed)
- [ ] Пользователь может приложить фото меню в `POST /api/ModerationShops`
- [ ] После parse капучино с ценой 9 становится `Present` / `9.00` / BYN
- [ ] Напиток не с фото остаётся `Unknown`, не `Absent`
- [ ] Среднее ~8 BYN → `Moderate`; меньше 7 → `Cheap`; больше 9 → `Expensive`
- [ ] `GET /api/CoffeeShops/{id}` содержит `menu` с `capturedAtUtc` после публикации
- [ ] PUT меняет цену/availability; `updatedAtUtc` обновляется
- [ ] Фото меню нет в галерее `photos[]`
- [ ] Custom/unmatched не видны клиенту

---

## Interview / locks

| Тема | Решение |
|------|---------|
| Что в меню v1 | Только стандартные кофейные напитки каталога |
| Авторские | Схема + unmatched, не API |
| Парсер | Gemini vision JSON |
| Очередь OSM | Продукт = модерация; роуты import не трогаем |
| 8 BYN | Moderate в коридоре 7–9 |
| Кто правит | Moderator/admin; юзер только фото на create |
| Клиенту | Деталка + дата загрузки |
