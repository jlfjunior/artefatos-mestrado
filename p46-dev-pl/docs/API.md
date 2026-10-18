# API Reference

## Transactions Service (Port 5001)

Base URL: `http://localhost:5001/api`

### POST /transactions
Criar novo lançamento (débito ou crédito).

**Request:**
```json
{
  "amount": 150.50,
  "type": "Credit",  // "Debit" ou "Credit"
  "description": "Venda de produtos",
  "transactionDate": "2025-06-16"
}
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 150.50,
  "type": "Credit",
  "description": "Venda de produtos",
  "transactionDate": "2025-06-16T00:00:00Z",
  "createdAt": "2025-06-16T14:30:00Z",
  "isProcessed": false
}
```

**Validações:**
- `amount` > 0
- `type` é "Debit" ou "Credit"
- `transactionDate` não pode ser no futuro

**Erros:**
- `400 Bad Request`: Validação falhou
- `500 Internal Server Error`: Erro interno

---

### GET /transactions
Listar lançamentos com paginação e filtro por data.

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `startDate` (date, optional)
- `endDate` (date, optional)

**Example:**
```
GET /transactions?pageNumber=1&pageSize=10&startDate=2025-06-01&endDate=2025-06-30
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "amount": 150.50,
      "type": "Credit",
      "description": "Venda de produtos",
      "transactionDate": "2025-06-16T00:00:00Z",
      "createdAt": "2025-06-16T14:30:00Z",
      "isProcessed": false
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 45,
  "totalPages": 5
}
```

---

### GET /transactions/{id}
Obter detalhes de uma transação específica.

**Parameters:**
- `id` (uuid): ID da transação

**Example:**
```
GET /transactions/550e8400-e29b-41d4-a716-446655440000
```

**Response (200 OK):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 150.50,
  "type": "Credit",
  "description": "Venda de produtos",
  "transactionDate": "2025-06-16T00:00:00Z",
  "createdAt": "2025-06-16T14:30:00Z",
  "isProcessed": false
}
```

**Erros:**
- `404 Not Found`: Transação não encontrada

---

### GET /health
Health check do serviço.

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "messageQueue": "Healthy"
  },
  "timestamp": "2025-06-16T14:30:00Z"
}
```

---

## Consolidation Service (Port 5002)

Base URL: `http://localhost:5002/api`

### GET /consolidations/{date}
Obter consolidação (saldo) de um dia específico.

**Parameters:**
- `date` (date): Data em formato YYYY-MM-DD

**Example:**
```
GET /consolidations/2025-06-16
```

**Response (200 OK):**
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "consolidationDate": "2025-06-16",
  "debitTotal": 200.00,
  "creditTotal": 350.50,
  "dailyBalance": 150.50,
  "processedCount": 3,
  "lastUpdatedAt": "2025-06-16T14:35:00Z"
}
```

**Significado dos campos:**
- `debitTotal`: Soma de todos os débitos do dia
- `creditTotal`: Soma de todos os créditos do dia
- `dailyBalance`: Saldo do dia (créditos - débitos)
- `processedCount`: Quantas transações foram consolidadas

**Erros:**
- `404 Not Found`: Nenhuma transação para essa data

---

### GET /consolidations
Listar consolidações com filtro por período.

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 20, max: 100)
- `startDate` (date, optional)
- `endDate` (date, optional)

**Example:**
```
GET /consolidations?startDate=2025-06-01&endDate=2025-06-30&pageNumber=1&pageSize=10
```

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": "660e8400-e29b-41d4-a716-446655440000",
      "consolidationDate": "2025-06-16",
      "debitTotal": 200.00,
      "creditTotal": 350.50,
      "dailyBalance": 150.50,
      "processedCount": 3,
      "lastUpdatedAt": "2025-06-16T14:35:00Z"
    },
    {
      "id": "770e8400-e29b-41d4-a716-446655440000",
      "consolidationDate": "2025-06-15",
      "debitTotal": 100.00,
      "creditTotal": 200.00,
      "dailyBalance": 100.00,
      "processedCount": 2,
      "lastUpdatedAt": "2025-06-15T23:59:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 30,
  "totalPages": 3
}
```

---

### GET /health
Health check do serviço de consolidação.

**Response (200 OK):**
```json
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "kafka": "Healthy"
  },
  "timestamp": "2025-06-16T14:30:00Z"
}
```

---

## Fluxo End-to-End

### Exemplo Completo

1. **Criar Transação de Crédito**
```bash
curl -X POST http://localhost:5001/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 500.00,
    "type": "Credit",
    "description": "Recebimento de cliente",
    "transactionDate": "2025-06-16"
  }'
```

2. **Criar Transação de Débito**
```bash
curl -X POST http://localhost:5001/api/transactions \
  -H "Content-Type: application/json" \
  -d '{
    "amount": 150.00,
    "type": "Debit",
    "description": "Pagamento de fornecedor",
    "transactionDate": "2025-06-16"
  }'
```

3. **Aguardar alguns segundos** (processamento assíncrono)

4. **Obter Consolidação**
```bash
curl http://localhost:5002/api/consolidations/2025-06-16
```

Resultado:
```json
{
  "consolidationDate": "2025-06-16",
  "debitTotal": 150.00,
  "creditTotal": 500.00,
  "dailyBalance": 350.00,
  "processedCount": 2
}
```

---

## Error Handling

Todos os erros seguem este formato:

```json
{
  "code": "ERROR_CODE",
  "message": "Descrição do erro",
  "details": {}
}
```

### Códigos de Erro

| HTTP | Code | Significado |
|------|------|-------------|
| 400 | VALIDATION_ERROR | Dados inválidos |
| 404 | NOT_FOUND | Recurso não existe |
| 500 | INTERNAL_ERROR | Erro interno do servidor |
| 503 | SERVICE_UNAVAILABLE | Serviço indisponível |

---

## Rate Limiting & Throttling

Não implementado ainda. Planejado para fase 2.

Recomendações:
- **Transações**: 100 req/s por IP
- **Consolidações**: 50 req/s por IP

---

## CORS

Habilitado para todas as origins em desenvolvimento:

```
Access-Control-Allow-Origin: *
Access-Control-Allow-Methods: GET, POST, PUT, DELETE
Access-Control-Allow-Headers: Content-Type
```

Em produção, configurar origins específicos.

---

## Swagger/OpenAPI

- Transactions: http://localhost:5001/swagger
- Consolidations: http://localhost:5002/swagger

Especificação completa em `specs/api_spec.yaml`

---

## Performance

### Transações Service
- Criação: ~50ms (com RabbitMQ publish)
- Listagem: ~30ms (com paginação)

### Consolidação Service
- Get por data: ~20ms (com cache)
- Listagem: ~40ms (com paginação)

> Tempos podem variar conforme BD e carga

---

## Changelog

### v1.0.0 (2025-06-16)
- ✅ Endpoints de Transações
- ✅ Endpoints de Consolidação
- ✅ Health checks
- ✅ Paginação
- ✅ Filtro por data

### v1.1.0 (Planejado)
- [ ] Autenticação JWT
- [ ] Rate limiting
- [ ] Redis cache
- [ ] GraphQL

---

## Exemplos de Código

### C# HttpClient

```csharp
var client = new HttpClient { BaseAddress = new Uri("http://localhost:5001") };

// Criar transação
var request = new { amount = 150.50, type = "Credit", transactionDate = DateTime.UtcNow };
var json = JsonSerializer.Serialize(request);
var content = new StringContent(json, Encoding.UTF8, "application/json");

var response = await client.PostAsync("/api/transactions", content);
var result = await response.Content.ReadAsAsync<TransactionDto>();
```

### Python Requests

```python
import requests

# Criar transação
url = "http://localhost:5001/api/transactions"
payload = {
    "amount": 150.50,
    "type": "Credit",
    "transactionDate": "2025-06-16"
}
response = requests.post(url, json=payload)
print(response.json())

# Listar consolidações
url = "http://localhost:5002/api/consolidations"
response = requests.get(url, params={"startDate": "2025-06-01", "endDate": "2025-06-30"})
print(response.json())
```

### JavaScript Fetch

```javascript
// Criar transação
const response = await fetch('http://localhost:5001/api/transactions', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    amount: 150.50,
    type: 'Credit',
    transactionDate: '2025-06-16'
  })
});
const data = await response.json();
console.log(data);
```

---

**Última atualização**: 2025-06-16
