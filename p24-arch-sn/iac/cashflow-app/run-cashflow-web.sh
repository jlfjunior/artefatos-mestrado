#!/bin/sh

cd ./web/ || exit

dotnet Bc.CashFlow.Web.dll --urls "http://*:${HTTP_PORT}"
