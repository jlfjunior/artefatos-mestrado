FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DailyConsolidated/CashFlow.DailyConsolidated.API/CashFlow.DailyConsolidated.API.csproj", "DailyConsolidated/CashFlow.DailyConsolidated.API/"]
COPY ["DailyConsolidated/CashFlow.DailyConsolidated.Domain/CashFlow.DailyConsolidated.Domain.csproj", "DailyConsolidated/CashFlow.DailyConsolidated.Domain/"]
COPY ["DailyConsolidated/CashFlow.DailyConsolidated.Application/CashFlow.DailyConsolidated.Application.csproj", "DailyConsolidated/CashFlow.DailyConsolidated.Application/"]
COPY ["DailyConsolidated/CashFlow.DailyConsolidated.Infrastructure/CashFlow.DailyConsolidated.Infrastructure.csproj", "DailyConsolidated/CashFlow.DailyConsolidated.Infrastructure/"]
RUN dotnet restore "DailyConsolidated/CashFlow.DailyConsolidated.API/CashFlow.DailyConsolidated.API.csproj"
COPY . .
WORKDIR "/src/DailyConsolidated/CashFlow.DailyConsolidated.API"
RUN dotnet build "CashFlow.DailyConsolidated.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CashFlow.DailyConsolidated.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.DailyConsolidated.API.dll"]