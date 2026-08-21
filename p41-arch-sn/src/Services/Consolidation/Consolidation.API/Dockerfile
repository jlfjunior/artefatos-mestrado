FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Consolidation/Consolidation.API/Consolidation.API.csproj", "src/Services/Consolidation/Consolidation.API/"]
COPY ["src/Services/Consolidation/Consolidation.Application/Consolidation.Application.csproj", "src/Services/Consolidation/Consolidation.Application/"]
COPY ["src/Services/Consolidation/Consolidation.Domain/Consolidation.Domain.csproj", "src/Services/Consolidation/Consolidation.Domain/"]
COPY ["src/Services/Consolidation/Consolidation.Infrastructure/Consolidation.Infrastructure.csproj", "src/Services/Consolidation/Consolidation.Infrastructure/"]

RUN dotnet restore "src/Services/Consolidation/Consolidation.API/Consolidation.API.csproj"

COPY . .

RUN dotnet build "src/Services/Consolidation/Consolidation.API/Consolidation.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/Consolidation/Consolidation.API/Consolidation.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Consolidation.API.dll"]