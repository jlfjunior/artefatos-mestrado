# Build a partir da raiz do repositório:
#   docker build -f src/Consolidado/Consolidado.Api/Dockerfile -t consolidado-api .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# global.json não é copiado de propósito: ele existe só para evitar o SDK 8
# preview na máquina local. Dentro da imagem o SDK 8 é o final.
COPY src/BuildingBlocks/Contracts/Contracts.csproj src/BuildingBlocks/Contracts/
COPY src/Consolidado/Consolidado.Domain/Consolidado.Domain.csproj src/Consolidado/Consolidado.Domain/
COPY src/Consolidado/Consolidado.Application/Consolidado.Application.csproj src/Consolidado/Consolidado.Application/
COPY src/Consolidado/Consolidado.Infrastructure/Consolidado.Infrastructure.csproj src/Consolidado/Consolidado.Infrastructure/
COPY src/Consolidado/Consolidado.Api/Consolidado.Api.csproj src/Consolidado/Consolidado.Api/
RUN dotnet restore src/Consolidado/Consolidado.Api/Consolidado.Api.csproj

COPY src/ src/
RUN dotnet publish src/Consolidado/Consolidado.Api/Consolidado.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# curl é usado pelo healthcheck do container (bate em /health).
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Consolidado.Api.dll"]
