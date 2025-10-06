#!/bin/sh

echo "Waiting for database service..."
sh /setup/wait-for.sh "${DB_HOSTNAME}" "${DB_PORT}"

echo "Giving some time for the database service to get itself together"
sh /setup/hold-off.sh "${HOLD_OFF_TIME}"

echo "Performing migrations for master database with sa user"
sh /setup/evolve-master-sa.sh

echo "Performing migrations for BCCF database with sa user"
sh /setup/evolve-bccf-sa.sh

echo "Performing migrations for Hangfire database with sa user"
sh /setup/evolve-hangfire-sa.sh

echo "Performing migrations for cashflow"
sh /setup/evolve-cashflow.sh

echo "Waiting for cache service..."
sh /setup/wait-for.sh "${CACHE_HOSTNAME}" "${CACHE_PORT}"

echo "Waiting for queue service..."
sh /setup/wait-for.sh "${QUEUE_HOSTNAME}" "${QUEUE_PORT}"

CASHFLOW_APP_UPPER=$(echo "$CASHFLOW_APP" | tr '[:lower:]' '[:upper:]')

if [ "$CASHFLOW_APP_UPPER" = "WEB" ]; then
    echo "Starting up cashflow-web"
    sh /setup/run-cashflow-web.sh
elif [ "$CASHFLOW_APP_UPPER" = "SCHEDULER" ]; then
    echo "Starting up cashflow-scheduler"
    sh /setup/run-cashflow-scheduler.sh
else
    echo "Error: Invalid CASHFLOW_APP value '$CASHFLOW_APP'. Expected 'WEB' or 'SCHEDULER'."
    exit 1
fi
