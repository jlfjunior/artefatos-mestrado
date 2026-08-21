# Quick Reference - Comandos Rapidos

## Setup Inicial

```bash
# Clonar
git clone https://github.com/seu-usuario/fluxo-caixa.git
cd fluxo-caixa

# Restaurar dependencias
dotnet restore

# Subir infraestrutura (SQL Server + RabbitMQ)
# O banco CaixaDb e criado automaticamente pelo container sql-init
docker-compose up -d
```

---

## Rodando Servicos

### Via Docker Compose (recomendado)

```bash
docker-compose up -d
# Aguarde ~30s para todos ficarem healthy
docker-compose ps
```

### Localmente (debugging)

```bash
# Terminal 1 — precisa de docker-compose up -d sqlserver rabbitmq
cd services/Transactions.API
dotnet run
# http://localhost:5001/swagger

# Terminal 2
cd services/Consolidation.API
dotnet run
# http://localhost:5002/swagger
```

---

## Testes

```bash
# Todos os testes
dotnet test tests/

# Servico especifico
dotnet test tests/Transactions.API.Tests/
dotnet test tests/Consolidation.API.Tests/

# Com coverage
dotnet test tests/ /p:CollectCoverage=true

# Verboso
dotnet test tests/ -v normal
```

---

## APIs

### POST /api/transactions

```bash
curl -X POST http://localhost:5001/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "lojista": 1,
    "amount": 150.50,
    "type": "Credit",
    "description": "Venda",
    "transactionDate": "2025-06-16T00:00:00Z"
  }'
```

Valores de `lojista`: `1`=Loja Norte, `2`=Loja Sul, `3`=Loja Leste, `4`=Loja Oeste

### GET /api/transactions

```bash
curl "http://localhost:5001/api/transactions?pageSize=10&pageNumber=1"
```

### GET /api/consolidations/{date}

```bash
curl http://localhost:5002/api/consolidations/2025-06-16
```

### Health Checks

```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
```

---

## Docker

```bash
# Subir tudo
docker-compose up -d

# Subir infraestrutura apenas
docker-compose up -d sqlserver rabbitmq

# Ver logs em tempo real
docker-compose logs -f transactions-api
docker-compose logs -f consolidation-api

# Parar
docker-compose down

# Reset completo (apaga banco)
docker-compose down -v

# Rebuild da imagem
docker-compose build transactions-api
docker-compose build consolidation-api

# Escalar Consolidation API
docker-compose up -d --scale consolidation-api=3
```

---

## Commits (Conventional Commits)

```bash
git commit -m "feat: adicionar endpoint de consolidacao"
git commit -m "fix: corrigir calculo de saldo"
git commit -m "docs: atualizar API reference"
git commit -m "test: adicionar testes para ConsolidationService"
git commit -m "refactor: extrair validacao para metodo privado"
git commit -m "chore: atualizar dependencias .NET"
```

---

## Debugging

### VS Code

Pressione `F5` — breakpoints funcionam normalmente.

### Ver Logs

```bash
# Arquivos de log locais
# Windows
type services\Transactions.API\logs\transactions-api-*.txt
type services\Consolidation.API\logs\consolidation-api-*.txt

# Docker
docker-compose logs -f transactions-api
docker-compose logs -f consolidation-api
```

---

## Estrutura de Diretorios

```
fluxo-caixa/
├── services/
│   ├── Shared/              # Contratos, DTOs, Enums (Lojista)
│   ├── Transactions.API/    # Port 5001
│   └── Consolidation.API/   # Port 5002
├── tests/                   # Unit tests
├── docs/                    # ADRs, API reference, Dev guide
├── SQL_CREATE_DATABASE.sql  # Script de banco
├── docker-compose.yml
├── ARCHITECTURE.md
└── README.md
```

---

## Documentacao

| Arquivo | Conteudo |
|---------|----------|
| README.md | Visao geral, setup, exemplos |
| ARCHITECTURE.md | Diagrama, padroes, schema do banco |
| docs/ADRs.md | 7 decisoes arquiteturais |
| docs/API.md | Endpoints, requests, responses |
| docs/DEVELOPMENT.md | Setup dev, testes, debugging |
| CONTRIBUTING.md | Como contribuir |

---

## Checklist Pre-Push

```bash
# 1. Testes passando
dotnet test tests/

# 2. Codigo compila
dotnet build

# 3. Branch atualizada
git pull origin main

# 4. Commit com mensagem clara
git commit -m "feat: descricao clara"

# 5. Push
git push origin feature/nome
```

---

## Problemas Comuns

### SQL Server demora a subir

```bash
# O sql-init aguarda automaticamente, mas se falhar:
docker-compose restart sql-init
docker-compose logs sql-init
```

### RabbitMQ nao inicia

```bash
docker-compose restart rabbitmq
docker-compose logs rabbitmq
# Management UI: http://localhost:15672 (guest/guest)
```

### Mensagens nao sendo consumidas

```bash
# Verificar filas no RabbitMQ Management UI
# http://localhost:15672 → Queues
docker-compose logs consolidation-api
docker-compose restart consolidation-api
```

### Porta em uso

```bash
# Alterar porta no docker-compose.yml
ports:
  - "5011:5001"
```

---

## Links Rapidos

- **Swagger Transactions**: http://localhost:5001/swagger
- **Swagger Consolidation**: http://localhost:5002/swagger
- **RabbitMQ Management**: http://localhost:15672 (guest/guest)
- **SQL Server**: localhost:1433 | Database: CaixaDb

---

## Exemplo de Fluxo Completo

```bash
# 1. Subir infraestrutura
docker-compose up -d

# 2. Aguardar ~30s e verificar saude
docker-compose ps

# 3. Criar transacao
curl -X POST http://localhost:5001/api/transactions \
  -H "Content-Type: application/json" \
  -d '{"lojista":1,"amount":100,"type":"Credit","transactionDate":"2025-06-16T00:00:00Z"}'

# 4. Aguardar alguns segundos (processamento assincrono)

# 5. Verificar consolidado
curl http://localhost:5002/api/consolidations/2025-06-16

# 6. Rodar testes
dotnet test tests/
```
