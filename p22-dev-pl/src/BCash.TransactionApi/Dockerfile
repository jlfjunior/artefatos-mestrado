FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

ENV PATH="$PATH:/root/.dotnet/tools"

COPY src/BCash.Domain/BCash.Domain.csproj ./BCash.Domain/
COPY src/BCash.Infrastructure/BCash.Infrastructure.csproj ./BCash.Infrastructure/
COPY src/BCash.Service/BCash.Service.csproj ./BCash.BCash.Service/
COPY src/BCash.TransactionApi/BCash.TransactionApi.csproj ./BCash.TransactionApi/

RUN dotnet restore ./BCash.TransactionApi/BCash.TransactionApi.csproj

COPY src/BCash.Domain/ ./BCash.Domain/
COPY src/BCash.Infrastructure/ ./BCash.Infrastructure/
COPY src/BCash.Service/ ./BCash.Service/
COPY src/BCash.TransactionApi/ ./BCash.TransactionApi/

WORKDIR /app/BCash.TransactionApi

RUN dotnet publish BCash.TransactionApi.csproj -c Release -o /out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /out .

EXPOSE 8082

CMD ["dotnet", "ef", "database", "update", "--project", "BCash.TransactionApi/TransactionApi.csproj"]

ENTRYPOINT ["dotnet", "BCash.TransactionApi.dll"]