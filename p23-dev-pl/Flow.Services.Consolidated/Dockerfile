FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia toda a solução (pasta raiz) para /src
COPY . ./

# Restaura usando a solução
RUN dotnet restore Flow.sln

# Publica o projeto de consolidação
RUN dotnet publish Flow.Services.Consolidated/Flow.Services.Consolidated.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Flow.Services.Consolidated.dll"]

