FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app/BCash.AuthApi

COPY *.csproj ./
RUN dotnet restore

COPY . ./
COPY ../ ../
RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /out .

EXPOSE 8081

ENTRYPOINT ["dotnet", "BCash.AuthApi.dll"]