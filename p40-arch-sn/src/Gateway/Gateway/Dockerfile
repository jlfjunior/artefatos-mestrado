FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Gateway/Gateway/Gateway.csproj", "src/Gateway/Gateway/"]

RUN dotnet restore "src/Gateway/Gateway/Gateway.csproj"

COPY . .

RUN dotnet build "src/Gateway/Gateway/Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Gateway/Gateway/Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.dll"]