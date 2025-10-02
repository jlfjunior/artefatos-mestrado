# Verity Challenge - Gerenciamento de Transações e Resumo Diário

## 📌 Visão Geral
Este projeto é um sistema baseado em microsserviços para gerenciar transações financeiras e gerar um resumo diário consolidado. O sistema foi projetado para garantir escalabilidade e resiliência, utilizando Clean Architecture, CQRS, MediatR, MassTransit (RabbitMQ), Entity Framework Core, Redis (Cache) e JWT para autenticação. Além disso, possui suporte a Swagger com autenticação e testes automatizados com NUnit.

🚨 Importante: O fluxo de autenticação atual não é o ideal para um ambiente de produção. O Keycloak seria a abordagem recomendada para gerenciar usuários e autenticação, permitindo maior flexibilidade e segurança, além de armazenar usuários em um banco de dados adequado. No entanto, para simplificação do desafio, foi implementada uma AuthAPI separada que utiliza JWT e Identity com SQLite.

🔹 **Fluxo Recomendado (Keycloak)**  
- O Keycloak gerenciaria a autenticação e autorização de forma centralizada.  
- A API de autenticação seria eliminada e as APIs consumiriam diretamente um Identity Provider (IdP) confiável.  
- O Keycloak permitiria usuários, papéis (roles) e permissões mais avançadas.  
- O sistema poderia se integrar com OAuth 2.0, OpenID Connect e LDAP para maior segurança.  

---

## 🚀 **Tecnologias Utilizadas**
- **C# (.NET 8)**
- **ASP.NET Core Web API**
- **Entity Framework Core (PostgreSQL & SQLite)**
- **Autenticação JWT**
- **Swagger com suporte a autenticação**
- **MediatR (Padrão CQRS)**
- **MassTransit (RabbitMQ)**
- **StackExchange.Redis (Cache)**
- **AutoMapper**
- **NUnit & Moq (Testes Unitários, InMemory Database)**
- **Docker & Docker Compose**


## 📌 **Futuras Melhorias**
🔹 **Keycloak** – Gerenciamento centralizado de usuários e autenticação  
🔹 **Polly** – Implementação de retries, circuit breakers e timeouts para resiliência  
🔹 **OpenTelemetry** – Tracing distribuído para monitoramento detalhado das requisições  
🔹 **Datadog** – Observabilidade e logs centralizados para melhor diagnóstico  
🔹 **Kubernetes (K8s)** – Orquestração e deploy escalável dos microsserviços  
🔹 **Frontend** para consumir as APIs  
🔹 **Testes de Carga** – Simulação de múltiplos usuários simultâneos com k6  
🔹 **Separação de Banco para CQRS** – Uso de bancos distintos para leitura e escrita, garantindo escalabilidade e performance:  
-  **Banco de Escrita** – PostgreSQL para operações transacionais  
-  **Banco de Leitura** – Replica otimizada para consultas rápidas (Event Sourcing ou caching avançado)  
-  **Sincronização Assíncrona** – Atualização entre bancos via eventos no RabbitMQ  

---

## 📂 Estrutura do Projeto

```
Verity.Challenge
│── src/
│   │
│   ├── Verity.Challenge.AuthAPI/         # API de Autenticação (Identity + JWT)
│   │   ├── Controllers/                   # Endpoints de Login e Registro
│   │   ├── Data/                          # Configuração do banco SQLite + Identity
│   │   ├── Migrations/                    # Migrações do banco de dados
│   │   ├── Models/                        # Modelos de requisição e usuários
│   │   ├── appsettings.json                # Configuração da API
│   │   ├── Program.cs                      # Configuração principal
│   │
│   ├── Verity.Challenge.Transactions/    # Microsserviço de Transações Financeiras
│   │   ├── Web.Api/                        # Camada de Controllers, Middlewares e Configurações
│   │   ├── Application/                    # Camada de Aplicação (CQRS, Handlers, DTOs)
│   │   ├── Domain/                         # Camada de Domínio (Entidades e Regras de Negócio)
│   │   ├── Infrastructure/                 # Infraestrutura (Banco, Mensageria, Configurações)
│   │
│   ├── Verity.Challenge.DailySummary/    # Microsserviço de Resumo Diário
│   │   ├── Web.Api/
│   │   ├── Application/
│   │   ├── Domain/
│   │   ├── Infrastructure/
│   │
│   ├── Shared/                           # Código compartilhado entre microsserviços
│   │   ├── Enums/                         # Enumerações compartilhadas
│   │   ├── Messages/                      # Mensagens para RabbitMQ
│   │
│── tests/
│   │
│   ├── Verity.Challenge.DailySummary.Tests/  # Testes do Resumo Diário
│   │   ├── Consumers/                         # Testes de Mensageria
│   │   ├── Domain/                            # Testes de Domínio
│   │   ├── Handlers/                          # Testes dos Handlers CQRS
│   │   ├── BaseTests.cs                       # Configuração base para testes
│   │
│   ├── Verity.Challenge.Transactions.Tests/  # Testes das Transações
│   │   ├── Domain/
│   │   ├── Handlers/
│   │   ├── BaseTests.cs
│
│── docker-compose.yml                     # Configuração do Docker para dependências
```

---

## ⚙️ **Configuração e Execução**

### **1️⃣ Pré-requisitos**
Antes de rodar o projeto, certifique-se de ter instalado:
- [.NET SDK 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)

### **2️⃣ Configuração do Banco de Dados**
Por padrão, o projeto utiliza **PostgreSQL**. Se estiver rodando localmente sem Docker, configure a conexão no arquivo **`appsettings.Development.json`**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=verity_transactions;Username=admin;Password=admin"
  },
  "RabbitMQ": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

Caso esteja utilizando **Docker**, as dependências podem ser iniciadas com:

```sh
docker-compose up -d
```

### **3️⃣ Aplicando as Migrations**
Após configurar o banco, execute as migrations para garantir que o esquema de dados esteja atualizado. No Package Manager Console do Visual Studio, selecione o projeto correto (Infrastructure) antes de rodar o comando:

```sh
update-database
```
Execute para cada microsserviço:

![image](https://github.com/user-attachments/assets/2daae9fc-fb66-43d8-9fbc-a08b66ed8060)

---

### **4️⃣ Executando a Aplicação**
Para rodar os microsserviços, pode serguir esse exemplo usando o Visual Studio.
Botão direito na solution > Propriedades:

![image](https://github.com/user-attachments/assets/b2fe36a7-0f6b-4f21-ad99-08355b3d9846)


Rode em múltiplos projetos:
![image](https://github.com/user-attachments/assets/e1f559ba-d054-4c8c-9ef6-a2d4fe2d18f5)


Inicie a aplicação:

![image](https://github.com/user-attachments/assets/6ea579eb-ec85-492c-9c2c-689a715ec1de)


Swagger Transaction:

![image](https://github.com/user-attachments/assets/964933fa-9f88-41d2-8923-da928ab11922)


Swagger Daily Summary:

![image](https://github.com/user-attachments/assets/716006d1-5464-4708-b237-a3a54ea2ad47)

Swagger Auth:
![image](https://github.com/user-attachments/assets/0c79e039-a183-4712-b1c2-dcb6621bcff7)

Logar com o usuário salvo no banco SQLite:

```json
{
  "username": "admin",
  "password": "VerityChallenge@123"
}
```

## 🔄 **Arquitetura e Fluxo**
O sistema utiliza o **padrão CQRS** para separar **operações de leitura e escrita**, garantindo maior **desempenho e escalabilidade**. Além disso, usa **RabbitMQ** para comunicação assíncrona entre microsserviços e **Redis** para otimizar a recuperação de dados.

1. O **usuário se autentica** na **AuthAPI**, que gera um **token JWT** para autorização nas APIs.
2. O **microsserviço de Transações** recebe uma requisição para **criar/editar/deletar** uma transação.
3. Após a persistência no banco, um **evento é publicado no RabbitMQ**.
4. O **microsserviço de Resumo Diário** consome essa mensagem para **atualizar o saldo diário**.
5. Para melhorar o desempenho, os dados do resumo diário são **armazenados em cache no Redis**.
6. Quando uma requisição de leitura é feita, o sistema **verifica primeiro no cache** antes de buscar no banco de dados.

🚀 Isso garante **eficiência, escalabilidade e menor latência** no acesso às informações!

---

## ✅ **Testes**
O projeto contém **testes unitários** utilizando **NUnit e Moq** utilizando banco em memória.

Para rodar os testes, execute:

```sh
dotnet test
```

ou utilize sua IDE de preferência. Exemplo no Visual Studio:

![image](https://github.com/user-attachments/assets/ba91e6b6-aac2-450b-b753-1f0795bce838)
