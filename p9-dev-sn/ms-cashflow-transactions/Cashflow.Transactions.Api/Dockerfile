FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /app
COPY ["*.sln", "./"]
COPY ["Cashflow.Transactions.Api/Cashflow.Transactions.Api.csproj", "Cashflow.Transactions.Api/"]
COPY ["Cashflow.Transactions.Application/Cashflow.Transactions.Application.csproj", "Cashflow.Transactions.Application/"]
COPY ["Cashflow.Transactions.Domain/Cashflow.Transactions.Domain.csproj", "Cashflow.Transactions.Domain/"]
COPY ["Cashflow.Transactions.Data/Cashflow.Transactions.Data.csproj", "Cashflow.Transactions.Data/"]
RUN dotnet restore "./Cashflow.Transactions.Api/Cashflow.Transactions.Api.csproj"
COPY . .
WORKDIR "/src/Cashflow.Transactions.Api"
RUN dotnet build "./Cashflow.Transactions.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "./Cashflow.Transactions.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cashflow.Transactions.Api.dll"]