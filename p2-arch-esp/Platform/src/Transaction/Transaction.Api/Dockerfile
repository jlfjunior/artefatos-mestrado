FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/Transaction/Transaction.Api/Transaction.Api.csproj Transaction/Transaction.Api/
COPY src/Transaction/Transaction.Application/Transaction.Application.csproj Transaction/Transaction.Application/
COPY src/Transaction/Transaction.Infrastructure/Transaction.Infrastructure.csproj Transaction/Transaction.Infrastructure/
COPY src/Commons/Commons.Infra.RabbitMQ/Commons.Infra.RabbitMQ.csproj Commons/Commons.Infra.RabbitMQ/

WORKDIR /src/Transaction/Transaction.Api
RUN dotnet restore

WORKDIR /src
COPY src/ .

WORKDIR /src/Transaction/Transaction.Api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "Transaction.Api.dll"]