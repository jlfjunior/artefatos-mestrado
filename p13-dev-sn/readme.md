🚀 Executando com Docker
Pré-requisitos
Docker Desktop

WSL2 habilitado (Windows)

Comandos
bash
Copiar
Editar
# Subir a aplicação + SQL Server via Docker Compose
docker compose up --build
🔧 O backend estará disponível em: http://localhost:5000/swagger

⚙️ Migrations EF Core
Para aplicar migrations localmente:

dotnet ef migrations add InitialCreate -p FluxoCaixa.Infrastructure -s FluxoCaixa.WebApi
dotnet ef database update -p FluxoCaixa.Infrastructure -s FluxoCaixa.WebApi
✅ Testes Automatizados
Execute os testes unitários e de integração com:

dotnet test
🧪 Exemplos de Endpoints
Criar Lançamento

POST /api/lancamentos
Content-Type: application/json
{
  "data": "2025-04-07",
  "valor": 150.00,
  "tipo": 1 // 1 = Receita, 2 = Despesa
}
Consultar Lançamentos

GET /api/lancamentos
📦 Variáveis de Ambiente
appsettings.json
json

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sqlserver;Database=FluxoCaixaDb;User Id=sa;Password=YourStrong!Passw0rd;"
  }
}
Use a mesma string no Docker para conectar com o container do SQL Server.

🧪 Testes de Integração com Mocks
Os testes de integração utilizam mocks e banco em memória (ou SQLite in-memory) para simular operações reais.

