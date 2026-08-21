# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CashFlow.sln global.json ./
COPY src/ ./src/
COPY tests/ ./tests/

RUN dotnet restore
RUN dotnet publish src/CashFlow.Api/CashFlow.Api.csproj -c Release -o /app/api --no-restore
RUN dotnet publish src/CashFlow.Consumer.Consolidation/CashFlow.Consumer.Consolidation.csproj -c Release -o /app/consumer-consolidation --no-restore
RUN dotnet publish src/CashFlow.Web/CashFlow.Web.csproj -c Release -o /app/web --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /app/api .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CashFlow.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS consumer-consolidation
WORKDIR /app
COPY --from=build /app/consumer-consolidation .
ENTRYPOINT ["dotnet", "CashFlow.Consumer.Consolidation.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS web
WORKDIR /app
COPY --from=build /app/web .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "CashFlow.Web.dll"]
