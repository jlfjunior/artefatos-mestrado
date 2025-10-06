FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia toda a solução (pasta raiz) para /src
COPY . ./

# Restaura usando a solução
RUN dotnet restore Flow.sln

# Publica o projeto de transações
RUN dotnet publish Flow.Services.Transactions/Flow.Services.Transactions.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Flow.Services.Transactions.dll"]
