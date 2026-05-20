---
phase: 05-test-coverage
plan: 02
subsystem: account-application-tests
tags: [tests, unit-tests, email, update-profile]
dependency_graph:
  requires: []
  provides: [UpdateEmailRequestHandler test coverage]
  affects: [CoffeePeek.Account.Application.Tests]
tech_stack:
  added: []
  patterns: [xUnit v3, Moq, FluentAssertions, static handler testing via direct call]
key_files:
  created:
    - CoffeePeek.Account.Application.Tests/Features/User/UpdateUserProfile/UpdateEmail/UpdateEmailRequestHandlerTest.cs
  modified: []
decisions:
  - CreateUser via DomainUser.Register + reflection Id override — consistent with UpdateUsernameHandlerTest pattern
metrics:
  duration: 3m
  completed_date: "2026-05-20"
  tasks_completed: 1
  files_created: 1
requirements: [TEST-05]
---

# Phase 5 Plan 02: UpdateEmailRequestHandler Tests Summary

4 unit-тестов для UpdateEmailRequestHandler покрывают успешный путь, NotFoundException, DomainException и идемпотентность при email того же пользователя.

## What Was Built

Добавлен файл `UpdateEmailRequestHandlerTest.cs` с 4 тестами в пространстве имён `CoffeePeek.Account.Application.Tests.Features.User.UpdateUserProfile.UpdateEmail`.

Тесты напрямую вызывают статический метод `UpdateEmailRequestHandler.Handle(...)` с мок-зависимостями через Moq, проверяя все ветви логики обработчика.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Тесты UpdateEmailRequestHandler | 9721408 | UpdateEmailRequestHandlerTest.cs (создан) |

## Test Results

```
Пройдено!  : не пройдено 0, пройдено 145, пропущено 0, всего 145
```

- Было тестов до: 141
- Добавлено новых: 4
- Итого: 145, все зелёные

## Test Cases

1. **Handle_WhenUserExists_AndEmailFree_UpdatesEmailAndReturnsSuccess** — проверяет `response.IsSuccess`, `event.UserId`, `event.Email`, `event.ConfirmationToken`, вызовы `Update` и `SaveChangesAsync`
2. **Handle_WhenUserNotFound_ThrowsNotFoundException** — NotFoundException с сообщением "User not found"
3. **Handle_WhenEmailTakenByAnotherUser_ThrowsDomainException** — DomainException с сообщением "Email is already taken"
4. **Handle_WhenEmailBelongsToSameUser_UpdatesSuccessfully** — нет исключения когда `existingOwner.Id == request.UserId` (идемпотентность)

## Deviations from Plan

Нет — план выполнен точно как описано.

## Known Stubs

Нет.

## Threat Flags

Нет новых security-поверхностей — только тестовый файл.

## Self-Check: PASSED

- [x] `UpdateEmailRequestHandlerTest.cs` существует по правильному пути
- [x] Коммит `9721408` подтверждён в `git log`
- [x] 145 тестов проходят, 0 failed
