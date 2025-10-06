# Architecture Decision Record 010 - Business

Adotando nova camada `Bc.CashFlow.Business` para ser um gateway para os `Controllers`, de forma cada controller fazer
uso de uma única implementação em `Business` e esta implementação lidar com quaisquer atores no sistema, como a
amarração entre transação, conta e tipo de conta, que afetam diretamente os valores a serem armazenados nos registros
das transações.
