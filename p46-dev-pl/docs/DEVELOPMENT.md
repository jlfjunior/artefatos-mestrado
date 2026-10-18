# Guia de Desenvolvimento

## Setup Inicial

### 1. Clonar o Repositório
```bash
git clone https://github.com/seu-usuario/fluxo-caixa.git
cd fluxo-caixa
```

### 2. Restaurar Dependências
```bash
dotnet restore
```

### 3. Configurar Banco de Dados Local

**Opção A: Com Docker Compose (Recomendado)**
```bash
docker-compose up -d sqlserver
# Aguarde ~10 segundos para SQL Server iniciar
```

**Opção B: SQL Server Localmente Instalado**
Modificar `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CaixaDb;User Id=sa;Password=seu_password;TrustServerCertificate=true;"
  }
}
```

### 4. Aplicar Migrations

**Transactions API:**
```bash
cd services/Transactions.API
dotnet ef database update
cd ../..
```

**Consolidation API:**
```bash
cd services/Consolidation.API
dotnet ef database update
cd ../..
```

---

## Rodando os Serviços

### Terminal 1: Transactions API
```bash
cd services/Transactions.API
dotnet run
# Rodando em http://localhost:5001
```

### Terminal 2: Consolidation API
```bash
cd services/Consolidation.API
dotnet run
# Rodando em http://localhost:5002
```

### Terminal 3: RabbitMQ + Kafka
```bash
docker-compose up -d rabbitmq kafka zookeeper
```

### Terminal 4: Testes
```bash
dotnet test tests/ --verbosity normal
```

---

## Estrutura de Pastas

```
fluxo-caixa/
├── services/
│   ├── Shared/                 # DTOs e Events compartilhados
│   ├── Transactions.API/       # Serviço de Transações
│   │   ├── src/
│   │   │   ├── Domain/         # Entidades (Transaction, TransactionType)
│   │   │   ├── Application/    # TransactionService
│   │   │   ├── Infrastructure/ # DbContext, Repositories
│   │   │   └── Presentation/   # TransactionsController
│   │   ├── Program.cs          # Startup, DI config
│   │   ├── appsettings.json
│   │   └── Dockerfile
│   └── Consolidation.API/      # Serviço de Consolidação
│       ├── src/
│       │   ├── Domain/         # Entidades (Consolidation)
│       │   ├── Application/    # ConsolidationService
│       │   ├── Infrastructure/ # DbContext, Consumers
│       │   └── Presentation/   # ConsolidationsController
│       ├── Program.cs
│       ├── appsettings.json
│       └── Dockerfile
├── tests/
│   ├── Transactions.API.Tests/
│   │   └── Unit/
│   │       ├── Domain/         # TransactionTests
│   │       └── Application/    # TransactionServiceTests
│   └── Consolidation.API.Tests/
│       └── Unit/
│           ├── Domain/         # ConsolidationTests
│           └── Application/    # ConsolidationServiceTests
├── docs/
│   ├── ADRs.md                # Decisões arquiteturais
│   └── API.md                 # Referência de API
├── specs/
│   └── api_spec.yaml          # OpenAPI spec
├── docker-compose.yml
├── ARCHITECTURE.md
└── README.md
```

---

## Padrões e Convenções

### Nomenclatura

**Pastas:**
```
src/Domain/Models/Transaction.cs
src/Domain/Interfaces/ITransactionRepository.cs
src/Application/Services/TransactionService.cs
src/Infrastructure/Data/TransactionDbContext.cs
src/Infrastructure/Repositories/TransactionRepository.cs
src/Presentation/Controllers/TransactionsController.cs
```

**Classes:**
- Entidades: `Transaction`, `Consolidation`
- Interfaces: `ITransactionRepository`, `IConsolidationService`
- Serviços: `TransactionService`, `ConsolidationService`
- Controllers: `TransactionsController`, `ConsolidationsController`
- DbContext: `TransactionDbContext`, `ConsolidationDbContext`

**Variáveis:**
```csharp
// camelCase para parâmetros e variables locais
public async Task CreateAsync(CreateTransactionRequest request)
{
    var transaction = Transaction.Create(...);
    var result = await _repository.AddAsync(transaction);
}

// PascalCase para properties
public decimal Amount { get; set; }
public TransactionType Type { get; set; }
```

---

## Entity Framework Core

### Criar Nova Migration

**Transactions.API:**
```bash
cd services/Transactions.API
dotnet ef migrations add AddNewField
dotnet ef database update
```

**Consolidation.API:**
```bash
cd services/Consolidation.API
dotnet ef migrations add AddNewField
dotnet ef database update
```

### Ver Migrations Aplicadas
```bash
dotnet ef migrations list
```

### Reverter Última Migration
```bash
dotnet ef migrations remove
dotnet ef database update <previous-migration-name>
```

---

## Testes

### Executar Todos os Testes
```bash
dotnet test tests/
```

### Testes de Serviço Específico
```bash
dotnet test tests/Transactions.API.Tests/
dotnet test tests/Consolidation.API.Tests/
```

### Com Cobertura
```bash
dotnet test tests/ /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Ver Resultado
```bash
dotnet test tests/ --logger "console;verbosity=detailed"
```

### Estrutura de um Teste
```csharp
[Fact]  // ou [Theory]
public async Task CreateAsync_WithValidData_ShouldReturnDto()
{
    // Arrange
    var input = new CreateTransactionRequest { /* ... */ };
    var mockRepo = new Mock<ITransactionRepository>();
    
    // Act
    var result = await _service.CreateAsync(input);
    
    // Assert
    result.Should().NotBeNull();
    result.Amount.Should().Be(input.Amount);
}
```

---

## Logging

### Usar em Serviços
```csharp
public class TransactionService
{
    private readonly ILogger<TransactionService> _logger;
    
    public async Task CreateAsync(CreateTransactionRequest request)
    {
        _logger.LogInformation("Criando transação: {Amount} {Type}", 
            request.Amount, request.Type);
        
        try
        {
            // ... lógica
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar transação");
            throw;
        }
    }
}
```

### Níveis de Log
```csharp
_logger.LogDebug("Detalhes para debugging");
_logger.LogInformation("Evento importante");
_logger.LogWarning("Algo inesperado");
_logger.LogError(ex, "Erro crítico");
_logger.LogCritical("Sistema inoperante");
```

### Ver Logs
```bash
# Em desenvolvimento (arquivo)
tail -f services/Transactions.API/logs/transactions-api-.txt

# Em container Docker
docker logs caixa-transactions-api -f
```

---

## Debugging

### VS Code

**launch.json automático:**
```bash
cd services/Transactions.API
dotnet new -d -all
# Procurar por tasks.json e launch.json
```

**Ou manual em `.vscode/launch.json`:**
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Transactions API",
            "type": "coreclr",
            "request": "launch",
            "program": "${workspaceFolder}/bin/Debug/net8.0/Transactions.API.dll",
            "args": [],
            "cwd": "${workspaceFolder}/services/Transactions.API",
            "stopAtEntry": false,
            "console": "internalConsole"
        }
    ]
}
```

**Pressionar F5 para iniciar debugging**

### Visual Studio

1. Abrir solution em Visual Studio
2. Selecionar projeto como startup
3. Pressionar F5

---

## Database Schema

### Transactions
```sql
CREATE TABLE Transactions (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    Amount DECIMAL(18,2) NOT NULL,
    Type INT NOT NULL,  -- 0=Debit, 1=Credit
    Description NVARCHAR(500),
    TransactionDate DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    IsProcessed BIT DEFAULT 0,
    INDEX IX_TransactionDate (TransactionDate),
    INDEX IX_CreatedAt (CreatedAt)
);
```

### Consolidations
```sql
CREATE TABLE Consolidations (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ConsolidationDate DATE NOT NULL UNIQUE,
    DebitTotal DECIMAL(18,2) DEFAULT 0,
    CreditTotal DECIMAL(18,2) DEFAULT 0,
    DailyBalance DECIMAL(18,2) DEFAULT 0,
    ProcessedCount INT DEFAULT 0,
    LastUpdatedAt DATETIME2 DEFAULT GETUTCDATE(),
    INDEX UX_ConsolidationDate (ConsolidationDate)
);
```

---

## Endpoints Úteis

### Health Checks
```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
```

### Swagger
- Transactions: http://localhost:5001/swagger
- Consolidations: http://localhost:5002/swagger

### RabbitMQ Management
```
http://localhost:15672
User: guest
Pass: guest
```

---

## Troubleshooting

### "Unable to connect to localhost:1433"
```bash
# Verificar se SQL Server está rodando
docker ps | grep sqlserver

# Se não estiver, iniciar
docker-compose up -d sqlserver

# Aguardar inicialização (~15 segundos)
sleep 15
```

### "Connection refused" em RabbitMQ
```bash
docker-compose up -d rabbitmq

# Aguardar inicialização (~10 segundos)
# Verificar em http://localhost:15672
```

### Erro em "dotnet ef database update"
```bash
# Restaurar dependências
dotnet restore

# Limpar builds antigos
dotnet clean

# Tentar novamente
dotnet ef database update
```

### Port em uso
```bash
# Mudar porta em appsettings.Development.json
{
  "urls": "http://+:5011"  // Ao invés de 5001
}
```

---

## Commits e Versionamento

### Conventional Commits
```bash
# Feature
git commit -m "feat: adicionar novo endpoint de consolidação"

# Fix
git commit -m "fix: corrigir cálculo de saldo diário"

# Docs
git commit -m "docs: atualizar README com instruções"

# Test
git commit -m "test: adicionar testes para TransactionService"

# Chore
git commit -m "chore: atualizar dependências"
```

### Branches
```bash
# Feature
git checkout -b feature/nova-funcionalidade

# Fix
git checkout -b fix/corrigir-bug

# Release
git checkout -b release/1.1.0
```

---

## Performance & Profiling

### Medir Latência
```bash
# Simples com curl
time curl http://localhost:5001/api/transactions

# Mais detalhado
curl -w "\nTime: %{time_total}s\n" http://localhost:5001/api/transactions
```

### Profiling com BenchmarkDotNet
```csharp
[MemoryDiagnoser]
public class TransactionServiceBenchmark
{
    [Benchmark]
    public async Task Create()
    {
        await _service.CreateAsync(new CreateTransactionRequest { /* */ });
    }
}
```

---

## Security Considerations

### Antes de Produção

- [ ] Alterar senha SQL Server
- [ ] Usar secrets para credenciais (Azure KeyVault)
- [ ] HTTPS em produção
- [ ] Adicionar JWT authentication
- [ ] CORS restritivo
- [ ] Rate limiting
- [ ] Input validation (já implementado)
- [ ] SQL injection protection (EF Core protege)

---

## Next Steps

1. ✅ Setup inicial
2. ✅ Rodar serviços
3. ✅ Executar testes
4. 📝 Implementar feature nova
5. 🧪 Adicionar testes
6. 📦 Commit & push
7. 🚀 Deploy

---

