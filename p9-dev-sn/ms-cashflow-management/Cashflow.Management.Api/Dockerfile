FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cashflow.Management.Api/Cashflow.Management.Api.csproj", "Cashflow.Management.Api/"]
COPY ["Cashflow.Management.Application/Cashflow.Management.Application.csproj", "Cashflow.Management.Application/"]
COPY ["Cashflow.Management.Domain/Cashflow.Management.Domain.csproj", "Cashflow.Management.Domain/"]
COPY ["Cashflow.Management.Data/Cashflow.Management.Data.csproj", "Cashflow.Management.Data/"]
RUN dotnet restore "./Cashflow.Management.Api/Cashflow.Management.Api.csproj"
COPY . .
WORKDIR "/src/Cashflow.Management.Api"
RUN dotnet build "./Cashflow.Management.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cashflow.Management.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cashflow.Management.Api.dll"]