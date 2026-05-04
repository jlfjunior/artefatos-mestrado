# src/Gateway/FluxoCaixa.Gateway/Dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/Gateway/FluxoCaixa.Gateway/FluxoCaixa.Gateway.csproj", "src/Gateway/FluxoCaixa.Gateway/"]
RUN dotnet restore "src/Gateway/FluxoCaixa.Gateway/FluxoCaixa.Gateway.csproj"

COPY . .
WORKDIR "/src/src/Gateway/FluxoCaixa.Gateway"
RUN dotnet build "FluxoCaixa.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoCaixa.Gateway.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
ENTRYPOINT ["dotnet", "FluxoCaixa.Gateway.dll"]
