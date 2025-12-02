FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.Photo.Api/CoffeePeek.Photo.Api.csproj", "CoffeePeek.Photo.Api/"]
COPY ["CoffeePeek.Photo.Infrastructure/CoffeePeek.Photo.Infrastructure.csproj", "CoffeePeek.Photo.Infrastructure/"]
COPY ["CoffeePeek.Shared.Models/CoffeePeek.Shared.Models.csproj", "CoffeePeek.Shared.Models/"]
COPY ["CoffeePeek.Photo.Core/CoffeePeek.Photo.Core.csproj", "CoffeePeek.Photo.Core/"]
COPY ["CoffeePeek.Photo.BuildingBlocks/CoffeePeek.Photo.BuildingBlocks.csproj", "CoffeePeek.Photo.BuildingBlocks/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
RUN dotnet restore "CoffeePeek.Photo.Api/CoffeePeek.Photo.Api.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.Photo.Api"
RUN dotnet build "CoffeePeek.Photo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CoffeePeek.Photo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.Photo.Api.dll"]
