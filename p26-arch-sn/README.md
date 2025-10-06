## Fluxo de Caixa

Esta é uma aplicação simples de fluxo de caixa, porém, que emprega boas práticas de arquitetura de software, nela são utilizados os conceitos de DDD, microsserviços, testes.

#### Desenho da solução:

</br>
<img src="https://github.com/raftriani/FluxoCaixa/blob/master/img/FluxoCaixa.drawio.png?raw=true" />
</br>

## Como rodar esse projeto

Como pré-requisitos, será necessário possuir o [Docker](https://www.docker.com/) instalado em sua máquina e o [Visual Studio](https://visualstudio.microsoft.com/pt-br/vs/) ou [VS Code](https://code.visualstudio.com/) com o [SDK do .NET Core 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) ou superior.

### Subindo os containers com Docker-Compose

Primeiramente, para subir as aplicações, através de um prompt de comando, vá até o diretório `Docker` na raíz do projeto, onde se encontra o arquivo `fluxo_de_caixa.yml` e rode o seguinte comando:

```powershell
docker-compose -f fluxo_de_caixa.yml up
```

Com isso todas as aplicações estarão funcionando, porém, ainda falta inicializar os bancos de dados relacionais.

### Inicializando os bancos de dados relacionais

Para iniciar o banco de dados da API de Lançamentos no Visual Studio, selecione o projeto **FluxoDeCaixa.Lancamentos.Infra** como **Default Project** et rode o seguinte comando:

```powershell
Update-Database -StartUpProject FluxoDeCaixa.Lancamentos.API
```

## Utilização das APIs

#### Informações da Conta

Request:

```powershell
GET /cashier/api/account/show-info HTTP/1.1
Host: localhost:5003
Authorization: Bearer [TOKEN]
```

Response:

```powershell
{
    "value": 0,
    "lastUpdate": "2023-09-04T14:04:30.7233618"
}
```

#### Lançamento de Crédito/Débito

Request:

```powershell
POST /cashier/api/account/add-entry HTTP/1.1
Host: localhost:5003
Authorization: Bearer [TOKEN]
Content-Type: application/json

{
    "Type": 1,
    "Value": 200,
    "Description": "Lancamento credito"
}
```

Response:

```powershell
{
    "message": "Lançamento criado com sucesso"
}
```

#### Relatório Diário Consolidado

Request:

```powershell
GET /reports/api/report/show-daily-report?month=10&year=2022&day=05 HTTP/1.1
Host: localhost:5003
Authorization: Bearer [TOKEN]
```

Response:

```powershell
[
    {
        "userId": "499DDC45-4912-42B9-996F-101473FC2042",
        "userName": "Guilherme",
        "entryType": 1,
        "entryValue": 300,
        "accountValueAfterEntry": 300,
        "entryDescription": "Primeiro teste",
        "entryDate": "2024-08-10T04:02:49.032Z"
    },
    {
        "userId": "499DDC45-4912-42B9-996F-101473FC2042",
        "userName": "Guilherme",
        "entryType": 1,
        "entryValue": 400,
        "accountValueAfterEntry": 800,
        "entryDescription": "Segundo teste",
        "entryDate": "2024-08-10T04:03:50.677Z"
    },
    {
        "userId": "499DDC45-4912-42B9-996F-101473FC2042",
        "userName": "Guilherme",
        "entryType": 1,
        "entryValue": 200,
        "accountValueAfterEntry": 1000,
        "entryDescription": "Terceiro este",
        "entryDate": "2024-08-10T04:14:59.233Z"
    }
]
```