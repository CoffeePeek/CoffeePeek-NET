FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://[::]:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.AuthService/CoffeePeek.AuthService.csproj", "CoffeePeek.AuthService/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Data/CoffeePeek.Data.csproj", "CoffeePeek.Data/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
RUN dotnet restore "CoffeePeek.AuthService/CoffeePeek.AuthService.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.AuthService"
RUN dotnet build "./CoffeePeek.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeePeek.AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.AuthService.dll"]
