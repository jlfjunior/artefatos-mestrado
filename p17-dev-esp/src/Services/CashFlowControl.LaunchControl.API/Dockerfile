# Use a imagem base do .NET 9.0 para execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 7253
EXPOSE 5166

# Usa imagem do SDK para construir o projeto
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia os arquivos de projeto
COPY ["src/Services/CashFlowControl.LaunchControl.API/CashFlowControl.LaunchControl.API.csproj", "src/Services/CashFlowControl.LaunchControl.API/"]
COPY ["src/Core/Application/CashFlowControl.Core.Application/CashFlowControl.Core.Application.csproj", "src/Core/Application/CashFlowControl.Core.Application/"]
COPY ["src/Core/Domain/CashFlowControl.Core.Domain/CashFlowControl.Core.Domain.csproj", "src/Core/Domain/CashFlowControl.Core.Domain/"]
COPY ["src/Infrastructure/CashFlowControl.LaunchControl.Infrastructure/CashFlowControl.LaunchControl.Infrastructure.csproj", "src/Infrastructure/CashFlowControl.LaunchControl.Infrastructure/"]
COPY ["src/Core/Infrastructure/CashFlowControl.Core.Infrastructure/CashFlowControl.Core.Infrastructure.csproj", "src/Core/Infrastructure/CashFlowControl.Core.Infrastructure/"]

# Restaura os pacotes do NuGet
RUN dotnet restore "./src/Services/CashFlowControl.LaunchControl.API/CashFlowControl.LaunchControl.API.csproj"

# Copia todo o código fonte
COPY . .

# Define o diretório de trabalho para build
WORKDIR "/src/src/Services/CashFlowControl.LaunchControl.API"

# Compila a aplicação
RUN dotnet build "./CashFlowControl.LaunchControl.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publica a aplicação
FROM build AS publish
RUN dotnet publish "./CashFlowControl.LaunchControl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Cria a imagem final para execução
FROM base AS final
WORKDIR /app

# Copia arquivos da publicação
COPY --from=publish /app/publish .

# Copia appsettings.json para o contêiner
COPY src/Services/CashFlowControl.DailyConsolidation.API/appsettings.json ./appsettings.json

# Copiar os certificados para o container
COPY certs/cert.pfx /https/cert.pfx
COPY certs/cert.crt /usr/local/share/ca-certificates/cert.crt

# Adicionar o certificado ao sistema (para evitar erros de confiança)
RUN update-ca-certificates

RUN ls /https/cert.pfx
RUN ls /usr/local/share/ca-certificates/cert.crt


# Define variáveis de ambiente
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="http://+:5166;https://+:7253"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/cert.pfx
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=102030@Certs
ENV urlApiAuth="https://cashflowcontrol.permissions.api:7043"
ENV Jwt__SecretKey="b3Bt0k3n!$@reM@nd4t0ryF0rJWT_32C"
ENV Jwt__Issuer="https://cashflowcontrol.permissions.api"
ENV Jwt__Audience="https://cashflowcontrol.LaunchControl.api"

ENTRYPOINT ["dotnet", "CashFlowControl.LaunchControl.API.dll"]