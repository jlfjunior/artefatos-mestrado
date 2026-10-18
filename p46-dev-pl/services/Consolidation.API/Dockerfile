# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["services/Consolidation.API/Consolidation.API.csproj", "services/Consolidation.API/"]
COPY ["services/Shared/Shared.csproj", "services/Shared/"]

# Restore
RUN dotnet restore "services/Consolidation.API/Consolidation.API.csproj"

# Copy remaining source
COPY . .

# Build
RUN dotnet build "services/Consolidation.API/Consolidation.API.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "services/Consolidation.API/Consolidation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# curl é necessário para o healthcheck do docker-compose
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

EXPOSE 5002
ENV ASPNETCORE_URLS=http://+:5002

ENTRYPOINT ["dotnet", "Consolidation.API.dll"]
