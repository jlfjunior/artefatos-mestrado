FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ENV PATH="$PATH:/root/.dotnet/tools"

COPY src/BCash.Domain/BCash.Domain.csproj ./BCash.Domain/
COPY src/BCash.Infrastructure/BCash.Infrastructure.csproj ./BCash.Infrastructure/
COPY src/BCash.Service/BCash.Service.csproj ./BCash.BCash.Service/
COPY src/BCash.ReportApi/BCash.ReportApi.csproj ./BCash.ReportApi/

RUN dotnet restore ./BCash.ReportApi/BCash.ReportApi.csproj

COPY src/BCash.Domain/ ./BCash.Domain/
COPY src/BCash.Infrastructure/ ./BCash.Infrastructure/
COPY src/BCash.Service/ ./BCash.Service/
COPY src/BCash.ReportApi/ ./BCash.ReportApi/

WORKDIR /app/BCash.ReportApi

RUN dotnet publish BCash.ReportApi.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /out .

EXPOSE 8083

ENTRYPOINT ["dotnet", "BCash.ReportApi.dll"]