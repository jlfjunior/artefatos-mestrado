# src/Services/FluxoCaixa.Consolidado/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/Services/FluxoCaixa.Consolidado.Api/FluxoCaixa.Consolidado.Api.csproj", "src/Services/FluxoCaixa.Consolidado.Api/"]
COPY ["src/Services/FluxoCaixa.Consolidado.Application/FluxoCaixa.Consolidado.Application.csproj", "src/Services/FluxoCaixa.Consolidado.Application/"]
COPY ["src/Services/FluxoCaixa.Consolidado.Domain/FluxoCaixa.Consolidado.Domain.csproj", "src/Services/FluxoCaixa.Consolidado.Domain/"]
COPY ["src/Services/FluxoCaixa.Consolidado.Infrastructure/FluxoCaixa.Consolidado.Infrastructure.csproj", "src/Services/FluxoCaixa.Consolidado.Infrastructure/"]
RUN dotnet restore "src/Services/FluxoCaixa.Consolidado.Api/FluxoCaixa.Consolidado.Api.csproj"

# Build all
COPY . .
WORKDIR "/src/src/Services/FluxoCaixa.Consolidado.Api"
RUN dotnet build "FluxoCaixa.Consolidado.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoCaixa.Consolidado.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "FluxoCaixa.Consolidado.Api.dll"]
