FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/ControleLancamentos.Domain/ControleLancamentos.Domain.csproj", "src/ControleLancamentos.Domain/"]
COPY ["src/ControleLancamentos.Application/ControleLancamentos.Application.csproj", "src/ControleLancamentos.Application/"]
COPY ["src/ControleLancamentos.Infrastructure/ControleLancamentos.Infrastructure.csproj", "src/ControleLancamentos.Infrastructure/"]
COPY ["src/ControleLancamentos.Api/ControleLancamentos.Api.csproj", "src/ControleLancamentos.Api/"]

RUN dotnet restore "src/ControleLancamentos.Api/ControleLancamentos.Api.csproj"

COPY . .
WORKDIR "/src/src/ControleLancamentos.Api"
RUN dotnet publish "ControleLancamentos.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ControleLancamentos.Api.dll"]
