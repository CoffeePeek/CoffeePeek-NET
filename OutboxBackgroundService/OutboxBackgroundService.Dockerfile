FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://[::]:80
ENV DOTNET_RUNNING_IN_CONTAINER=true


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OutboxBackgroundService/OutboxBackgroundService.csproj", "OutboxBackgroundService/"]
COPY ["CoffeePeek.AuthService/CoffeePeek.AuthService.csproj", "CoffeePeek.AuthService/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Data/CoffeePeek.Data.csproj", "CoffeePeek.Data/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
RUN dotnet restore "OutboxBackgroundService/OutboxBackgroundService.csproj"
COPY . .
WORKDIR "/src/OutboxBackgroundService"
RUN dotnet build "./OutboxBackgroundService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OutboxBackgroundService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false /p:PublishContentFiles=false \
    && rm -f /app/publish/appsettings.json /app/publish/appsettings.Development.json \
    && find /app/publish -name "appsettings*.json" ! -path "*/OutboxBackgroundService/*" -delete || true

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OutboxBackgroundService.dll"]