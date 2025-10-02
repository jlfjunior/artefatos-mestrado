# Cashflow Web

Este é o projeto front-end do Cashflow App.

> Angular 19

## Etapas para executar

Na pasta do projeto `fe-cashflow-web`.

Execute um `npm install` para instalar as dependencias do projeto.

Execute `ng serve` para rodar a aplicação local. 

Acesse a aplicação no endereço: `http://localhost:4200/`.

![image](https://github.com/user-attachments/assets/79665ce6-428b-4abe-87d5-1ad6196d286f)


## Dicas

Caso precise ajustar o endereço das apis ou do keycloack, pode realizar a mudança no arquivo `fe-cashflow-web/src/environments/environment.ts`.
> Por padrão estás são as variaveis de ambiente definidas no environment
>
> keycloackRealmUrl: `http://localhost:8080/realms/cashflow/protocol/openid-connect/token`
>
> keycloackClientId: `cashflow-web`
>
> keycloackGrantType: `password`
>
> cashflowTransactionApi: `http://localhost:5282/api`
>
> cashflowManagementApi:  `http://localhost:5251/api`
