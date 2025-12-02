FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.Api/CoffeePeek.Api.csproj", "CoffeePeek.Api/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Infrastructure/CoffeePeek.Infrastructure.csproj", "CoffeePeek.Infrastructure/"]
COPY ["CoffeePeek.BuildingBlocks/CoffeePeek.BuildingBlocks.csproj", "CoffeePeek.BuildingBlocks/"]
COPY ["CoffeePeek.BusinessLogic/CoffeePeek.BusinessLogic.csproj", "CoffeePeek.BusinessLogic/"]
COPY ["CoffeePeek.Domain/CoffeePeek.Domain.csproj", "CoffeePeek.Domain/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffeePeek.Shared.Models/CoffeePeek.Shared.Models.csproj", "CoffeePeek.Shared.Models/"]

RUN dotnet restore "CoffeePeek.Api/CoffeePeek.Api.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.Api"
RUN dotnet build "CoffeePeek.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CoffeePeek.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.Api.dll"]
