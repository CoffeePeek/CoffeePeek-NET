FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.MediaService/CoffeePeek.MediaService.csproj", "CoffeePeek.MediaService/"]
RUN dotnet restore "CoffeePeek.MediaService/CoffeePeek.MediaService.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.MediaService"
RUN dotnet build "./CoffeePeek.MediaService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeePeek.MediaService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.MediaService.dll"]
