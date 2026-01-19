FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OutboxBackgroundService/OutboxBackgroundService.csproj", "OutboxBackgroundService/"]
COPY ["CoffeePeek.Shops.Domain/CoffeePeek.Shops.Domain.csproj", "CoffeePeek.Shops.Domain/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
COPY ["CoffeePeek.Shops.Application/CoffeePeek.Shops.Application.csproj", "CoffeePeek.Shops.Application/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffePeek.ServiceDefaults/CoffePeek.ServiceDefaults.csproj", "CoffePeek.ServiceDefaults/"]
COPY ["CoffeePeek.Account.Domain/CoffeePeek.Account.Domain.csproj", "CoffeePeek.Account.Domain/"]
COPY ["CoffeePeek.Moderation.Domain/CoffeePeek.Moderation.Domain.csproj", "CoffeePeek.Moderation.Domain/"]
COPY ["CoffeePeek.Moderation.Application/CoffeePeek.Moderation.Application.csproj", "CoffeePeek.Moderation.Application/"]
COPY ["CoffeePeek.Shops.Infrastructure/CoffeePeek.Shops.Infrastructure.csproj", "CoffeePeek.Shops.Infrastructure/"]
COPY ["CoffeePeek.Account.Infrastructure/CoffeePeek.Account.Infrastructure.csproj", "CoffeePeek.Account.Infrastructure/"]
COPY ["CoffeePeek.Account.Application/CoffeePeek.Account.Application.csproj", "CoffeePeek.Account.Application/"]
COPY ["CoffeePeek.Moderation.Infrastructure/CoffeePeek.Moderation.Infrastructure.csproj", "CoffeePeek.Moderation.Infrastructure/"]
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
