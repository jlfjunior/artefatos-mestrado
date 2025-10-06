#!/bin/sh

cd ./scheduler/ || exit

dotnet Bc.CashFlow.Scheduler.dll --urls "http://*:${HTTP_PORT}"
