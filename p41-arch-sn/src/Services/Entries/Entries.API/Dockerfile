FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Services/Entries/Entries.API/Entries.API.csproj", "src/Services/Entries/Entries.API/"]
COPY ["src/Services/Entries/Entries.Application/Entries.Application.csproj", "src/Services/Entries/Entries.Application/"]
COPY ["src/Services/Entries/Entries.Domain/Entries.Domain.csproj", "src/Services/Entries/Entries.Domain/"]
COPY ["src/Services/Entries/Entries.Infrastructure/Entries.Infrastructure.csproj", "src/Services/Entries/Entries.Infrastructure/"]

RUN dotnet restore "src/Services/Entries/Entries.API/Entries.API.csproj"

COPY . .

RUN dotnet build "src/Services/Entries/Entries.API/Entries.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/Entries/Entries.API/Entries.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Entries.API.dll"]