# src/Services/FluxoCaixa.Lancamentos/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/Services/FluxoCaixa.Lancamentos.Api/FluxoCaixa.Lancamentos.Api.csproj", "src/Services/FluxoCaixa.Lancamentos.Api/"]
COPY ["src/Services/FluxoCaixa.Lancamentos.Application/FluxoCaixa.Lancamentos.Application.csproj", "src/Services/FluxoCaixa.Lancamentos.Application/"]
COPY ["src/Services/FluxoCaixa.Lancamentos.Domain/FluxoCaixa.Lancamentos.Domain.csproj", "src/Services/FluxoCaixa.Lancamentos.Domain/"]
COPY ["src/Services/FluxoCaixa.Lancamentos.Infrastructure/FluxoCaixa.Lancamentos.Infrastructure.csproj", "src/Services/FluxoCaixa.Lancamentos.Infrastructure/"]
RUN dotnet restore "src/Services/FluxoCaixa.Lancamentos.Api/FluxoCaixa.Lancamentos.Api.csproj"

# Build all
COPY . .
WORKDIR "/src/src/Services/FluxoCaixa.Lancamentos.Api"
RUN dotnet build "FluxoCaixa.Lancamentos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoCaixa.Lancamentos.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "FluxoCaixa.Lancamentos.Api.dll"]
