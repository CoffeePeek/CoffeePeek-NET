FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://[::]:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj", "CoffeePeek.ModerationService/"]
COPY ["Coffeepeek.Moderation.Application/Coffeepeek.Moderation.Application.csproj", "Coffeepeek.Moderation.Application/"]
COPY ["CoffeePeek.Moderation.Domain/CoffeePeek.Moderation.Domain.csproj", "CoffeePeek.Moderation.Domain/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffePeek.ServiceDefaults/CoffePeek.ServiceDefaults.csproj", "CoffePeek.ServiceDefaults/"]
COPY ["CoffeePeek.Moderation.Infrastructure/CoffeePeek.Moderation.Infrastructure.csproj", "CoffeePeek.Moderation.Infrastructure/"]
RUN dotnet restore "CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.ModerationService"
RUN dotnet build "./CoffeePeek.ModerationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeePeek.ModerationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.ModerationService.dll"]
