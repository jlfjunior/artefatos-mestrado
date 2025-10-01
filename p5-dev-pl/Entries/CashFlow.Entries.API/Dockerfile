FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Entries/CashFlow.Entries.API/CashFlow.Entries.API.csproj", "Entries/CashFlow.Entries.API/"]
COPY ["Entries/CashFlow.Entries.Domain/CashFlow.Entries.Domain.csproj", "Entries/CashFlow.Entries.Domain/"]
COPY ["Entries/CashFlow.Entries.Application/CashFlow.Entries.Application.csproj", "Entries/CashFlow.Entries.Application/"]
COPY ["Entries/CashFlow.Entries.Infrastructure/CashFlow.Entries.Infrastructure.csproj", "Entries/CashFlow.Entries.Infrastructure/"]
RUN dotnet restore "Entries/CashFlow.Entries.API/CashFlow.Entries.API.csproj"
COPY . .
WORKDIR "/src/Entries/CashFlow.Entries.API"
RUN dotnet build "CashFlow.Entries.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CashFlow.Entries.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CashFlow.Entries.API.dll"]