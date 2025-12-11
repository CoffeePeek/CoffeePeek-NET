FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN adduser --disabled-password --gecos '' app
WORKDIR /app

ENV ASPNETCORE_URLS=http://[::]:80
ENV DOTNET_RUNNING_IN_CONTAINER=true
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.JobVacancies/CoffeePeek.JobVacancies.csproj", "CoffeePeek.JobVacancies/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffeePeek.Data/CoffeePeek.Data.csproj", "CoffeePeek.Data/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
RUN dotnet restore "CoffeePeek.JobVacancies/CoffeePeek.JobVacancies.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.JobVacancies"
RUN dotnet build "./CoffeePeek.JobVacancies.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeePeek.JobVacancies.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
USER app
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
CMD curl -f http://localhost:80/health || exit 1
ENTRYPOINT ["dotnet", "CoffeePeek.JobVacancies.dll"]
