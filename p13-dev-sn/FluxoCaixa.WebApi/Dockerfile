FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FluxoCaixa.WebApi/FluxoCaixa.WebApi.csproj", "FluxoCaixa.WebApi/"]
COPY ["FluxoCaixa.Application/FluxoCaixa.Application.csproj", "FluxoCaixa.Application/"]
COPY ["FluxoCaixa.Domain/FluxoCaixa.Domain.csproj", "FluxoCaixa.Domain/"]
COPY ["FluxoCaixa.Infrastructure/FluxoCaixa.Infrastructure.csproj", "FluxoCaixa.Infrastructure/"]

RUN dotnet restore "FluxoCaixa.WebApi/FluxoCaixa.WebApi.csproj"

COPY . .
WORKDIR "/src/FluxoCaixa.WebApi"
RUN dotnet build "FluxoCaixa.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoCaixa.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "FluxoCaixa.WebApi.dll"]
