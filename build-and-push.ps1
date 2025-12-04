# PowerShell скрипт для сборки и пуша всех Docker образов
# Использование: .\build-and-push.ps1

$ErrorActionPreference = "Stop"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Building and pushing Docker images" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# Функция для выполнения команды с обработкой ошибок
function Invoke-DockerCommand {
    param(
        [string]$Command,
        [string]$Description
    )
    
    Write-Host ""
    Write-Host ">>> $Description" -ForegroundColor Yellow
    Write-Host "Command: $Command" -ForegroundColor Gray
    
    $result = Invoke-Expression $Command
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to execute: $Description" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "[OK] Success: $Description" -ForegroundColor Green
}

# 1. AuthService
Invoke-DockerCommand `
    -Command "docker build -t winststreloc/coffeepeek.authservice:dev -f CoffeePeek.AuthService/AuthService.Dockerfile ." `
    -Description "Building AuthService image"

Invoke-DockerCommand `
    -Command "docker push winststreloc/coffeepeek.authservice:dev" `
    -Description "Pushing AuthService image"

# 1..1 OutboxBackgroundService
Invoke-DockerCommand `
    -Command "docker build -t winststreloc/coffeepeek.authservice.outbox-bg:dev -f OutboxBackgroundService/OutboxBackgroundService.Dockerfile ." `
    -Description "Building OutboxBackgroundService image"

Invoke-DockerCommand `
    -Command "docker push winststreloc/coffeepeek.authservice.outbox-bg:dev" `
    -Description "Pushing OutboxBackgroundService image"

# 2. UserService
Invoke-DockerCommand `
    -Command "docker build -t winststreloc/coffeepeek.userservice:dev -f CoffeePeek.UserService/UserService.Dockerfile ." `
    -Description "Building UserService image"

Invoke-DockerCommand `
    -Command "docker push winststreloc/coffeepeek.userservice:dev" `
    -Description "Pushing UserService image"

# 3. ShopsService
Invoke-DockerCommand `
    -Command "docker build -t winststreloc/coffeepeek.shopsservice:dev -f CoffeePeek.ShopsService/ShopsService.Dockerfile ." `
    -Description "Building ShopsService image"

Invoke-DockerCommand `
    -Command "docker push winststreloc/coffeepeek.shopsservice:dev" `
    -Description "Pushing ShopsService image"

# 4. ModerationService
Invoke-DockerCommand `
    -Command "docker build -t winststreloc/coffeepeek.moderationservice:dev -f CoffeePeek.ModerationService/ModerationService.Dockerfile ." `
    -Description "Building ModerationService image"

Invoke-DockerCommand `
    -Command "docker push winststreloc/coffeepeek.moderationservice:dev" `
    -Description "Pushing ModerationService image"

# 5. Nginx
Invoke-DockerCommand `
    -Command "docker build -t winststreloc/nginx:dev -f nginx/Dockerfile nginx/" `
    -Description "Building Nginx image"

Invoke-DockerCommand `
    -Command "docker push winststreloc/nginx:dev" `
    -Description "Pushing Nginx image"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "All images built and pushed successfully!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
