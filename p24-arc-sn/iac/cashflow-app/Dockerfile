ARG DOTNET_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build-env

WORKDIR /app

COPY ./src/cashflow ./

RUN dotnet restore
RUN dotnet build --no-restore
RUN dotnet test --no-restore --verbosity normal
RUN dotnet publish Bc.CashFlow.Web -c Release -o out/web --no-restore
RUN dotnet publish Bc.CashFlow.Scheduler -c Release -o out/scheduler --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}

ARG EVOLVE_VERSION="3.2.0"
ARG HOLD_OFF_TIME="15"
ENV EVOLVE_VERSION="${EVOLVE_VERSION}"
ENV HOLD_OFF_TIME="${HOLD_OFF_TIME}"
ENV DB_HOSTNAME=""
ENV DB_PORT=""
ENV DB_MASTER_SA_CONNECTION_STRING=""
ENV DB_MASTER_SA_LOCATION=""
ENV DB_BCCF_SA_CONNECTION_STRING=""
ENV DB_BCCF_SA_LOCATION=""
ENV DB_HANGFIRE_SA_CONNECTION_STRING=""
ENV DB_HANGFIRE_SA_LOCATION=""
ENV CASHFLOW_APP="WEB"
ENV HTTP_PORT="8080"

WORKDIR /setup
COPY ./iac/cashflow-app /setup/

RUN apt update
RUN apt install -y wget tar netcat-openbsd

RUN mkdir /evolve
RUN wget https://github.com/lecaillon/Evolve/releases/download/${EVOLVE_VERSION}/evolve_${EVOLVE_VERSION}_Linux-64bit.tar.gz -O evolve.tar.gz && \
    tar -xzf evolve.tar.gz -C /evolve && \
    rm evolve.tar.gz

RUN find /setup -name "*.sh" -exec sed -i 's/\r$//' {} \;
RUN chmod +x /setup/*

RUN rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=build-env /app/out .

EXPOSE ${HTTP_PORT}

RUN mkdir -p /root/.aspnet/DataProtection-Keys

ENTRYPOINT ["/setup/entrypoint.sh"]
