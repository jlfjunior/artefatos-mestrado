# Abrir o Package Manager Console;
# definir como default project: CashFlowControl.DailyConsolidation.API
# Rodar o comando para criação do 
# Criando a migração
dotnet ef migrations add InitialCreate --project src/Core/Infrastructure/CashFlowControl.Core.Infrastructure --startup-project src/Services/CashFlowControl.DailyConsolidation.API --output-dir Migrations



# Aplicando a migração ao banco de dados
dotnet ef database update --project src/Services/CashFlowControl.DailyConsolidation.API --startup-project src/Services/CashFlowControl.DailyConsolidation.API
