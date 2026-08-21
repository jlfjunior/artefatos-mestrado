#!/bin/bash
# Cria múltiplos bancos a partir da variável POSTGRES_MULTIPLE_DATABASES
# (lista separada por vírgula). Rodado uma vez no primeiro boot do Postgres.
set -e

if [ -n "$POSTGRES_MULTIPLE_DATABASES" ]; then
  for db in $(echo "$POSTGRES_MULTIPLE_DATABASES" | tr ',' ' '); do
    echo "Criando banco '$db'..."
    psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" <<-EOSQL
      SELECT 'CREATE DATABASE "$db"'
      WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db')\gexec
EOSQL
  done
fi
