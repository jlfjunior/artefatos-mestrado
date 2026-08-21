FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8000

ENV ASPNETCORE_URLS=http://+:8000

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Src/Challenger.EasyFlow.API/Challenger.EasyFlow.API.csproj", "Src/Challenger.EasyFlow.API/"]
RUN dotnet restore "Src/Challenger.EasyFlow.API/Challenger.EasyFlow.API.csproj"
COPY . .
WORKDIR "/src/Src/Challenger.EasyFlow.API"
RUN dotnet build "Challenger.EasyFlow.API.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Challenger.EasyFlow.API.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Challenger.EasyFlow.API.dll"]
