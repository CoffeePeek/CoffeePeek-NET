## CI/CD для CoffeePeek

### Общая схема

- **CI**:
  - Сборка и тесты .NET решения `CoffePeek.sln`.
  - Сборка и публикация Docker-образов для основных сервисов в Docker Hub.
- **CD**:
  - **DigitalOcean App Platform** (основной): использует Docker-образы из Docker Hub.
  - **Railway** (опционально): для обратной совместимости или резервного деплоя.
  - GitHub Actions после пуша образов выполняет job `deploy_digitalocean` для обновления компонентов на DigitalOcean.

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
- Job `deploy_digitalocean`:
  - Устанавливает `doctl` (DigitalOcean CLI).
  - Обновляет компоненты App Platform через DigitalOcean API.
  - Триггерит новый деплой с обновленными Docker-образами.
  - Компоненты должны быть настроены на использование образов вида:
    - `docker.io/<DOCKERHUB_USERNAME>/coffeepeek.authservice:<short-git-sha>`
    - и аналогично для остальных сервисов.
- Job `deploy_railway` (опционально):
  - Подготовлен для запуска Railway CLI/команд деплоя.
  - Можно отключить, установив `if: false` в workflow.

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

Таким образом, CI-часть (build + test + публикация образов) не зависит от конкретного провайдера и может быть переиспользована с любым другим провайдером, который умеет работать с Docker-образами.

### Настройка DigitalOcean App Platform

1. **Создание App Platform приложения**:
   - Перейдите в DigitalOcean Dashboard → App Platform
   - Создайте новое приложение
   - Используйте файл `.digitalocean/app.yaml` как основу для конфигурации
   - Или создайте компоненты вручную через веб-интерфейс

2. **Необходимые секреты в GitHub**:
   - `DIGITALOCEAN_ACCESS_TOKEN` - Personal Access Token из DigitalOcean
   - `DIGITALOCEAN_APP_ID` - ID приложения в App Platform (можно получить через `doctl apps list`)
   - `DOCKER_HUB_USERNAME` - уже настроен
   - `DOCKER_HUB_PASSWORD` - уже настроен

3. **Настройка компонентов**:
   - Каждый сервис должен быть настроен как отдельный компонент (Service)
   - Источник образа: Docker Hub
   - Репозиторий: `docker.io/<DOCKERHUB_USERNAME>/<service-name>`
   - Тег: `dev` (будет обновляться автоматически через API)

4. **Автоматический деплой**:
   - Workflow автоматически обновляет теги образов при каждом пуше
   - DigitalOcean App Platform автоматически подтянет новые образы при следующем деплое
   - Можно настроить webhook для автоматического триггера деплоя при обновлении образа

### Миграция с Railway на DigitalOcean

1. Создайте приложение в DigitalOcean App Platform
2. Настройте компоненты для каждого сервиса
3. Добавьте секреты `DIGITALOCEAN_ACCESS_TOKEN` и `DIGITALOCEAN_APP_ID` в GitHub
4. Workflow автоматически начнет деплоить на DigitalOcean
5. Опционально: отключите Railway deployment, установив `if: false` в job `deploy_railway`


