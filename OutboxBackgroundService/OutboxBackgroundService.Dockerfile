FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OutboxBackgroundService/OutboxBackgroundService.csproj", "OutboxBackgroundService/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
COPY ["CoffeePeek.AuthService/CoffeePeek.AuthService.csproj", "CoffeePeek.AuthService/"]
COPY ["CoffeePeek.Data/CoffeePeek.Data.csproj", "CoffeePeek.Data/"]
COPY ["CoffePeek.ServiceDefaults/CoffePeek.ServiceDefaults.csproj", "CoffePeek.ServiceDefaults/"]
COPY ["CoffeePeek.ShopsService/CoffeePeek.ShopsService.csproj", "CoffeePeek.ShopsService/"]
COPY ["CoffeePeek.Shared.Models/CoffeePeek.Shared.Models.csproj", "CoffeePeek.Shared.Models/"]
COPY ["CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj", "CoffeePeek.ModerationService/"]
RUN dotnet restore "OutboxBackgroundService/OutboxBackgroundService.csproj"
COPY . .
WORKDIR "/src/OutboxBackgroundService"
RUN dotnet build "./OutboxBackgroundService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OutboxBackgroundService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OutboxBackgroundService.dll"]