FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/CashFlow.API/CashFlow.API.csproj", "src/CashFlow.API/"]
RUN dotnet restore "src/CashFlow.API/CashFlow.API.csproj"
COPY . .
WORKDIR "/src/src/CashFlow.API"
RUN dotnet build "CashFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CashFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.API.dll"]
