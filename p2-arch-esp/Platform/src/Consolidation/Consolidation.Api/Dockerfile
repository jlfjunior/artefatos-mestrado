FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copie apenas os .csproj para cache eficiente
COPY src/Consolidation/Consolidation.Api/Consolidation.Api.csproj Consolidation/Consolidation.Api/
COPY src/Consolidation/Consolidation.Application/Consolidation.Application.csproj Consolidation/Consolidation.Application/
COPY src/Consolidation/Consolidation.Infrastructure/Consolidation.Infrastructure.csproj Consolidation/Consolidation.Infrastructure/
COPY src/Commons/Commons.Infra.RabbitMQ/Commons.Infra.RabbitMQ.csproj Commons/Commons.Infra.RabbitMQ/

WORKDIR /src/Consolidation/Consolidation.Api
RUN dotnet restore

WORKDIR /src
COPY src/ .

WORKDIR /src/Consolidation/Consolidation.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Consolidation.Api.dll"]