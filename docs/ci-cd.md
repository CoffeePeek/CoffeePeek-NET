## CI/CD для CoffeePeek

### Общая схема

- **CI**:
  - Сборка и тесты .NET решения `CoffePeek.sln`.
  - Сборка и публикация Docker-образов для основных сервисов в Docker Hub.
- **CD (сейчас)**:
  - Railway использует Docker-образы как источник деплоя.
  - GitHub Actions после пуша образов выполняет job `deploy_railway`, который рассчитан на использование Railway CLI/настроек.

### Docker-образы

Собираются образы:

- `coffeepeek.authservice`
- `coffeepeek.authservice.outbox-bg`
- `coffeepeek.userservice`
- `coffeepeek.shopsservice`
- `coffeepeek.moderationservice`
- `coffeepeek.jobvacancies`
- `gateway`

Образы тегируются так:

- `docker.io/<DOCKERHUB_USERNAME>/<image>:dev` – удобный дев-тег.
- `docker.io/<DOCKERHUB_USERNAME>/<image>:<short-git-sha>` – стабильный тег, привязанный к коммиту.

Имена и теги совпадают с тем, что можно использовать на любом другом провайдере (Kubernetes, VPS с docker-compose, другой PaaS).

### GitHub Actions workflow

Файл: `.github/workflows/ci-cd.yml`

- Job `build_and_test`:
  - `dotnet restore CoffePeek.sln`
  - `dotnet build CoffePeek.sln --configuration Release`
  - `dotnet test CoffePeek.sln --configuration Release`
- Job `build_and_push_images`:
  - Логин в Docker Hub с использованием секретов `DOCKERHUB_USERNAME` и `DOCKERHUB_TOKEN`.
  - Сборка и пуш Docker-образов всех сервисов с тегами `dev` и `<short-git-sha>`.
- Job `deploy_railway`:
  - Подготовлен для запуска Railway CLI/команд деплоя.
  - Railway-сервисы должны быть настроены на использование образов вида:
    - `docker.io/<DOCKERHUB_USERNAME>/coffeepeek.authservice:<short-git-sha>`
    - и аналогично для остальных сервисов.

### Перенос на другие платформы

- **VPS / docker-compose**:
  - Использовать те же образы из Docker Hub.
  - В `docker-compose.yml` указать `image: docker.io/<DOCKERHUB_USERNAME>/coffeepeek.shopsservice:<tag>`.
- **Kubernetes**:
  - В `Deployment` ресурсах использовать те же имена образов.
  - CI-часть не меняется, только манифесты и CD-слой.
- **Другие PaaS**:
  - Подключить Docker Hub как источник образов.
  - Настроить приложение на использование нужного тега образа.

Таким образом, CI-часть (build + test + публикация образов) не зависит от Railway и может быть переиспользована с любым другим провайдером, который умеет работать с Docker-образами.


