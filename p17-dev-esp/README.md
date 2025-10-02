# CashFlowControl

## Visão Geral
O **CashFlowControl** é uma aplicação desenvolvida para gerenciamento e controle de fluxo de caixa, focada em fornecer uma visão clara das entradas e saídas financeiras. O objetivo é ajudar empresas e indivíduos a monitorar suas finanças, tomar decisões embasadas e automatizar processos financeiros rotineiros.

## Tecnologias Utilizadas
A aplicação foi desenvolvida utilizando as seguintes tecnologias:

- **.NET 9** - Framework principal da aplicação
- **CQRS (Command Query Responsibility Segregation)** - Para separação clara de comandos e consultas
- **Entity Framework Core** - ORM para interação com o banco de dados
- **FluentValidation** - Biblioteca para validação de dados
- **Serilog** - Para logging estruturado e monitoramento
- **Mensageria (RabbitMQ / Amazon SQS)** - Para processamento assíncrono e comunicação entre serviços
- **Docker / Docker Compose** - Para containerização e fácil execução da aplicação em diferentes ambientes
- **API Gateway (Ocelot)** - Para gerenciar as chamadas entre os microserviços e fornecer um ponto centralizado de autenticação e autorização, não tem swagger configurado, mas pode utilizar a collection que está na raiz do projeto.
- **Autenticação e Autorização** - Implementação de controle de acesso com autenticação via JWT (JSON Web Token) e autorização baseada em roles
- **Nginx** - Usado como servidor reverso para gerenciar as requisições entre os microserviços, distribuindo o tráfego de forma eficiente e balanceada. O Nginx também é responsável por melhorar a performance e segurança da aplicação, fornecendo TLS/SSL para criptografia e autenticação.


## Arquitetura
O projeto segue uma arquitetura baseada em **DDD (Domain-Driven Design)**, **Clean Architecture** e **CQRS** para garantir separação de responsabilidades e facilitar a manutenção do código. Os principais componentes são:

- **Application**: Contém os casos de uso da aplicação.
- **Domain**: Define as entidades e regras de negócio.
- **Infrastructure**: Implementação de repositórios, conexão com banco de dados e serviços externos.
- **Services**: Contém os controllers e endpoints expostos via API.
- **API Gateway**: Gerencia as comunicações entre os microserviços e executa a autenticação e autorização.

## Decisões Tomadas
Durante o desenvolvimento, algumas decisões foram tomadas para garantir a escalabilidade, performance e manutenção do sistema:

1. **Uso do CQRS**: Para separar comandos e consultas, melhorando a performance e manutenção.
2. **Entity Framework**: O EF foi utilizado para operações de escrita e gestão de transações.
3. **RabbitMQ**: Para processamento assíncrono e escalabilidade do sistema, permitindo que grandes volumes de transações sejam processados sem impactar o desempenho da aplicação. A configuração inclui uma fila de **Dead Letter Queue (DLQ)** para garantir a consistência das mensagens e o tratamento de falhas.
4. **Serilog**: Implementado para logging estruturado, garantindo rastreabilidade e monitoramento eficiente.
5. **API Gateway**: Centralização das requisições e controle de acesso aos microserviços.
6. **Autenticação e Autorização**: Implementação de autenticação JWT e controle de acesso baseado em roles, garantindo a segurança e o controle de permissões na aplicação.
7. **Nginx**: Implementação do Nginx como servidor reverso para balanceamento de carga, roteamento eficiente das requisições para os microserviços e gerenciamento da segurança com TLS/SSL.


## Como Executar o Projeto

### 1. Configuração do Banco de Dados

- Certifique-se de que o banco de dados está configurado corretamente. O **Entity Framework Core** será utilizado para realizar a migração do banco de dados durante a execução inicial.
- Você pode configurar o banco de dados diretamente ou usar o Docker para inicializar o banco em um contêiner.
- Durante a criação do banco de dados também já estamos criando um usuário ADM para autenticação:
  Usuário: admin@cashflowcontrol.com
  Senha: Admin@123

### 2. Configuração das Dependências

- **Docker**: Certifique-se de ter o [Docker](https://www.docker.com/get-started) instalado em sua máquina. Usamos o `docker-compose` para orquestrar os serviços.
  
  - Clonar o repositório do projeto.
  - Na raiz do projeto, crie e configure os arquivos de ambiente necessários, como `docker-compose`, `dockerfile` e `appsettings.json`, contendo as configurações de banco de dados, RabbitMQ, API Gateway, etc.

  **Exemplo de execução do Docker Compose:**
  ```bash
  docker-compose up --build


### 3. Testes

- O projeto inclui testes unitários e de integração para garantir a qualidade do código. Para rodar os testes, utilize o seguinte comando:
**dotnet test**

  Isso executará todos os testes definidos no projeto, verificando a integridade da lógica da aplicação.

### 4. Collection Postman
- No diretório "CashFlowControl\Collection Postman\Cash Flow Control.postman_collection.json" está disponível collection do Postman para testes das APIs

**Diagrama da Aplicação**

![Cash Flow Contro](https://github.com/user-attachments/assets/97e87270-a5a8-4c82-8216-e9f6381564b0)

