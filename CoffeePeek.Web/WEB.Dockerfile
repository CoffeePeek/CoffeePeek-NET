FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

# Настройка для прослушивания на IPv6 (требуется для Railway приватной сети)
ENV ASPNETCORE_URLS=http://[::]:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Web не использует базу данных напрямую
# Если в будущем понадобится БД, добавьте:
# ENV PostgresCpOptions__ConnectionString=""

EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.Web/CoffeePeek.Web.csproj", "CoffeePeek.Web/"]
COPY ["CoffeePeek.Web.Contract/CoffeePeek.Web.Contract.csproj", "CoffeePeek.Web.Contract/"]
RUN dotnet restore "CoffeePeek.Web/CoffeePeek.Web.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.Web"
RUN dotnet build "./CoffeePeek.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeePeek.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.Web.dll"]
