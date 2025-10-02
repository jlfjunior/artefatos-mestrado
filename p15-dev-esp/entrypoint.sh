#!/bin/bash

# Iniciar o serviço de Launch
dotnet /app/launchservice/LaunchService.dll &

# Iniciar o serviço de Authentication
dotnet /app/authenticationservice/AuthenticationService.dll &

# Iniciar o serviço de Consolidation
dotnet /app/consolidationservice/ConsolidationService.dll &

# Manter o container ativo (aguarda os serviços rodando)
wait
