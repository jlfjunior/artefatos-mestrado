#!/bin/sh

# shellcheck disable=SC2086
/evolve/evolve_${EVOLVE_VERSION}_Linux-64bit/evolve migrate sqlserver -c "${DB_HANGFIRE_SA_CONNECTION_STRING}" -l ${DB_HANGFIRE_SA_LOCATION} -p user_login:${DB_HANGFIRE_USER_LOGIN} -p user_password:${DB_HANGFIRE_USER_PASSWORD}
