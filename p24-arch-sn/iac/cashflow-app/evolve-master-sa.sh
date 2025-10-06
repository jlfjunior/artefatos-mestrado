#!/bin/sh

# shellcheck disable=SC2086
/evolve/evolve_${EVOLVE_VERSION}_Linux-64bit/evolve migrate sqlserver -c "${DB_MASTER_SA_CONNECTION_STRING}" -l ${DB_MASTER_SA_LOCATION}
