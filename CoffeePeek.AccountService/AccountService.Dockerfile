FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 80


FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CoffeePeek.AccountService/CoffeePeek.AccountService.csproj", "CoffeePeek.AccountService/"]
COPY ["CoffeePeek.Account.Infrastructure/CoffeePeek.Account.Infrastructure.csproj", "CoffeePeek.Account.Infrastructure/"]
COPY ["CoffeePeek.Account.Application/CoffeePeek.Account.Application.csproj", "CoffeePeek.Account.Application/"]
COPY ["CoffeePeek.Account.Domain/CoffeePeek.Account.Domain.csproj", "CoffeePeek.Account.Domain/"]
COPY ["CoffeePeek.Shared.Infrastructure/CoffeePeek.Shared.Infrastructure.csproj", "CoffeePeek.Shared.Infrastructure/"]
COPY ["CoffeePeek.Contract/CoffeePeek.Contract.csproj", "CoffeePeek.Contract/"]
COPY ["CoffeePeek.Shared.Extensions/CoffeePeek.Shared.Extensions.csproj", "CoffeePeek.Shared.Extensions/"]
COPY ["CoffePeek.ServiceDefaults/CoffePeek.ServiceDefaults.csproj", "CoffePeek.ServiceDefaults/"]
RUN dotnet restore "CoffeePeek.AccountService/CoffeePeek.AccountService.csproj"
COPY . .
WORKDIR "/src/CoffeePeek.AccountService"
RUN dotnet build "./CoffeePeek.AccountService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CoffeePeek.AccountService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeePeek.AccountService.dll"]
