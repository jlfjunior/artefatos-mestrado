#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["./src/FluxoDeCaixa.Lancamentos.API/FluxoDeCaixa.Lancamentos.API.csproj", "FluxoDeCaixa.Lancamentos.API/"]
COPY ["./src/FluxoDeCaixa.Core/Core.csproj", "FluxoDeCaixa.Core/"]
RUN dotnet restore "./FluxoDeCaixa.Lancamentos.API/FluxoDeCaixa.Lancamentos.API.csproj"
COPY . .
WORKDIR "/src/src/FluxoDeCaixa.Lancamentos.API"
RUN dotnet build "FluxoDeCaixa.Lancamentos.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FluxoDeCaixa.Lancamentos.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FluxoDeCaixa.Lancamentos.API.dll"]