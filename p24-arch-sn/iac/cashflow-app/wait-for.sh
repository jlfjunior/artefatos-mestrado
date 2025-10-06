#!/bin/sh

HOST=$1
PORT=$2

while true; do
    nc -z "$HOST" "$PORT"
    
    if [ $? -eq 0 ]; then
        echo "${HOST}:${PORT} is available!"
        break
    else
        echo "${HOST}:${PORT} unavailable, trying again in 5 seconds..."
        sleep 5
    fi
done
