# Build a partir da raiz do repositório:
#   docker build -f src/Lancamentos/Lancamentos.Api/Dockerfile -t lancamentos-api .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# global.json não é copiado de propósito: ele existe só para evitar o SDK 8
# preview na máquina local. Dentro da imagem o SDK 8 é o final.
COPY src/BuildingBlocks/Contracts/Contracts.csproj src/BuildingBlocks/Contracts/
COPY src/Lancamentos/Lancamentos.Domain/Lancamentos.Domain.csproj src/Lancamentos/Lancamentos.Domain/
COPY src/Lancamentos/Lancamentos.Application/Lancamentos.Application.csproj src/Lancamentos/Lancamentos.Application/
COPY src/Lancamentos/Lancamentos.Infrastructure/Lancamentos.Infrastructure.csproj src/Lancamentos/Lancamentos.Infrastructure/
COPY src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj src/Lancamentos/Lancamentos.Api/
RUN dotnet restore src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj

COPY src/ src/
RUN dotnet publish src/Lancamentos/Lancamentos.Api/Lancamentos.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
# curl é usado pelo healthcheck do container (bate em /health).
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Lancamentos.Api.dll"]
