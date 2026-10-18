# controleDeLancamentos

Projeto desenvolvido para o desafio **desafio-arquiteto-software-out2024.pdf**, com foco em processamento de lançamentos financeiros (crédito e débito) e consulta de saldo diário consolidado com alta disponibilidade em leitura.

## 1. Visão Geral do Projeto

O **controleDeLancamentos** implementa dois fluxos de negócio principais:

- **Escrita de lançamentos** (débito/crédito) via API REST.
- **Leitura de consolidado diário** por data de referência.

O objetivo central é manter o endpoint de criação de lançamentos rápido e resiliente, mesmo sob carga, enquanto a consolidação diária ocorre de forma assíncrona e desacoplada.

## 2. Arquitetura e Padrões Adotados

### Clean Architecture

O código segue separação em camadas:

- **Domain**: entidades e regras de negócio puras (`Lancamento`, `ConsolidadoDiario`, regras de validação).
- **Application**: casos de uso, contratos, DTOs e interfaces.
- **Infrastructure**: EF Core, PostgreSQL, repositórios, event bus in-memory e worker em background.
- **API**: controllers, autenticação JWT, documentação Swagger/OpenAPI.

Essa organização reduz acoplamento, facilita testes unitários e preserva a regra de dependência (camadas externas dependem das internas).

### Princípios SOLID aplicados

- **S (Single Responsibility)**: serviços focados por caso de uso (`CriarLancamentoService`, `CalcularConsolidadoDiarioService`).
- **O (Open/Closed)**: contratos por interface permitem evolução sem alterar consumidores.
- **L (Liskov)**: implementações de repositórios respeitam os contratos da Application.
- **I (Interface Segregation)**: interfaces específicas para cada necessidade (ex.: escrita/leitura de consolidado).
- **D (Dependency Inversion)**: Application depende de abstrações; Infrastructure fornece implementações via DI.

## 3. Estratégia de Resiliência e Desacoplamento

Foi adotado um padrão **CQRS simplificado** com separação de modelo de escrita e leitura:

- **Write model**: tabela `lancamentos`.
- **Read model**: tabela `consolidados_diarios`.

Fluxo implementado:

1. `POST /api/lancamentos` persiste o lançamento na tabela `lancamentos`.
2. Após persistir, publica `LancamentoCriadoEvent` em um **Event Bus in-memory** (`System.Threading.Channels`).
3. Um `BackgroundService` consome o evento e executa upsert acumulado em `consolidados_diarios`.
4. `GET /api/consolidado-diario` consulta diretamente o read-model pré-calculado.

Com isso, a criação de lançamento fica **desacoplada** do cálculo de consolidado: se houver atraso, lock ou falha transitória na consolidação, o endpoint de lançamento continua disponível porque não depende do cálculo síncrono no mesmo request.

No requisito de pico (50 RPS), a leitura em tabela pré-calculada evita `SUM` em tempo real sobre todos os lançamentos e reduz custo de consulta para acesso direto por data, sustentando throughput com baixa latência e sem perda de requisição HTTP no cenário do desafio.

## 4. Desenho da Solução

```text
                    +----------------------------------+
                    |        Cliente / Consumidor      |
                    +----------------+-----------------+
                                     |
                                     v
                      +--------------+---------------+
                      |     API ASP.NET Core (.NET)  |
                      |  /api/lancamentos (POST)     |
                      |  /api/consolidado-diario GET |
                      +------+-------------------+----+
                             |                   |
                 (write path)|                   |(read path)
                             v                   v
                 +-----------+----+      +-------+------------------+
                 | Tabela: lancamentos|   | Tabela: consolidados_   |
                 | (write model)      |   | diarios (read model)    |
                 +-----------+--------+   +-------------------------+
                             |
                             | publica evento
                             v
                 +-----------+-------------------------------+
                 | InMemory Event Bus (System.Threading.     |
                 | Channels)                                 |
                 +-----------+-------------------------------+
                             |
                             | consome evento
                             v
                 +-----------+-------------------------------+
                 | BackgroundService: AtualizarConsolidado   |
                 | upsert/acúmulo por data de referência     |
                 +-------------------------------------------+
```

## 5. Tecnologias Utilizadas

- **.NET 10 / C#**
- **ASP.NET Core Web API**
- **Entity Framework Core 10**
- **PostgreSQL 16**
- **NUnit** (testes unitários)
- **Swashbuckle / Swagger**
- **Docker Compose**
- **pgAdmin 4**

## 6. Como Executar o Projeto Localmente

### Pré-requisitos

- Docker + Docker Compose

### Passo a passo

1. Na raiz do projeto, suba os containers:

```bash
docker compose up --build
```

2. Acesse os serviços:

- **Swagger (API):** http://localhost:8080/swagger
- **pgAdmin:** http://localhost:5050

3. Credenciais e variáveis de ambiente:

- O arquivo `.env` contém os valores de banco/JWT/pgAdmin usados no `docker-compose.yml`.
- Em ambientes reais, use secret manager/variáveis seguras e não commite segredos.

## 7. Autenticação (JWT)

Os endpoints de negócio (`/api/lancamentos` e `/api/consolidado-diario`) estão protegidos com JWT Bearer.

### Gerando token de teste via cURL

```bash
curl -X POST "http://localhost:8080/api/autenticacao/token-teste" \
  -H "Content-Type: application/json" \
  -d "{\"usuario\":\"arquiteto-teste\"}"
```

A resposta contém `accessToken` e `tokenType` (`Bearer`).

### Chamando endpoint protegido com Bearer

```bash
curl -X POST "http://localhost:8080/api/lancamentos" \
  -H "Authorization: Bearer SEU_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"valor\":150.75,\"tipo\":\"Credito\",\"dataLancamento\":\"2026-05-30T10:00:00\",\"descricao\":\"Recebimento\"}"
```

```bash
curl "http://localhost:8080/api/consolidado-diario?dataReferencia=2026-05-30" \
  -H "Authorization: Bearer SEU_TOKEN"
```

### Usando no Swagger UI

1. Gere o token em `POST /api/autenticacao/token-teste`.
2. Clique em **Authorize** no topo do Swagger.
3. Informe: `Bearer SEU_TOKEN`.
4. Confirme em **Authorize** e execute os endpoints protegidos.

## 8. Como Rodar os Testes

Para executar todos os testes NUnit:

```bash
dotnet test
```

Os testes cobrem regras de domínio de lançamentos e cenários de aplicação (criação/publicação de eventos e cálculo do consolidado diário).

## 9. Evoluções Futuras (Arquitetura Target)

Para um ambiente produtivo de maior escala e múltiplas instâncias, a evolução natural é substituir o Event Bus in-memory por broker durável, por exemplo:

- **RabbitMQ**
- **Azure Service Bus**
- **Kafta**

E complementar com padrões de confiabilidade operacional, como:

- **Outbox/Inbox** para consistência entre gravação e publicação.
- **Retry com backoff** e **dead-letter queue**.
- **Observabilidade** (métricas, tracing distribuído e alertas).
