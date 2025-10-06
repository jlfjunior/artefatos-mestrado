# Clean Architecture Sample Application

Este projeto é um exemplo de aplicação utilizando a arquitetura Clean Architecture. Ele demonstra a estrutura básica de um projeto seguindo os princípios dessa arquitetura, bem como o uso de algumas bibliotecas populares.

## DESCRITIVO DA SOLUÇÃO
Um comerciante precisa controlar o seu fluxo de caixa diário com os lançamentos
(débitos e créditos), também precisa de um relatório que disponibilize o saldo
diário consolidado.

## REQUISITOS DE NEGÓCIO:
- Serviço que faça o controle de lançamentos;
- Serviço do consolidado diário.


## Estrutura do Projeto

![Estrutura](https://raw.githubusercontent.com/Aciole/cash-flow/main/img/folders.png) 

Tecnologias Utilizadas

    FluentValidation: Biblioteca para validação de dados e regras de negócio.
    MediatR: Biblioteca para implementação de padrões de mediador e envio de comandos.
    MongoDB.Driver: Biblioteca oficial do MongoDB para interação com o banco de dados.
    xUnit: Framework de testes unitários para escrever e executar testes.
    ELK (Elasticsearch, Logstash, Kibana): Conjunto de ferramentas para armazenamento e análise de logs.
    APM Server: Servidor para coleta e visualização de dados de rastreamento e performance.

## Bibliotecas Utilizadas

- **FluentValidation**: Utilizado para a validação dos comandos recebidos pela aplicação.
- **MediatR**: Utilizado para implementar o padrão mediator, separando os comandos e os handlers.
- **MongoDB.Driver**: Utilizado para a persistência dos dados no MongoDB.
- **xUnit**: Utilizado para escrever os testes unitários da aplicação.
- **ELK Stack**: Utilizado para o logging centralizado, composto pelo Elasticsearch, Logstash e Kibana.
- **APM Server**: Utilizado para a captura e análise de dados de desempenho e rastreamento.


## Executando a Aplicação

Para executar a aplicação, siga as etapas abaixo:

1. Certifique-se de ter o Docker e o Docker Compose instalados em sua máquina.
2. Clone o repositório para o seu ambiente local.
3. Navegue até o diretório raiz do projeto.
4. Execute o comando `docker-compose up -d` para iniciar os containers do MongoDB, ELK Stack e APM Server.
5. Aguarde até que os containers sejam iniciados corretamente.
6. Execute o comando `dotnet run --project API` para iniciar a API da aplicação.
7. Acesse `http://localhost:5051/swagger` para visualizar a documentação da API.

## Casos de Uso da Aplicação

1. RegisterNewCashFlowUseCase: Cria o Caixa;
2. AddTransactionUseCase: Cria nova Transação;
3. CancelTransactionUseCase: Cancel/Reverte transação;
4. GetDailyBalanceUseCase: Consulta de lançamentos diarios;
5. GetByAccountIdAndDateRangeUseCase: Consulta de lançamentos com intervalo de datas;

lembrando que a clase ValidateCommandCoR, é executada antes de cada UseCase, podendo garantir o inteiramente, os conseitos do SOLID,
caso seja necessario criar novas validações é só cria e organizar a ordem de execução atraves da classe IoCExtension.cs;


## Considerações Finais

Este projeto de exemplo demonstra uma estrutura baseada na Clean Architecture e o uso de algumas bibliotecas populares para validação, persistência e testes. A aplicação segue o padrão de arquitetura em camadas, facilitando a manutenção, testabilidade e evolução do código.

Uma das vantagens da Clean Architecture é a sua flexibilidade, permitindo que a aplicação seja adaptada para atender a diferentes necessidades e requisitos. Se houver a necessidade de migrar para uma arquitetura orientada a eventos, isso pode ser alcançado através de algumas modificações no projeto.

Para migrar a aplicação para uma arquitetura orientada a eventos, você pode considerar o uso de um Message Broker, como o RabbitMQ ou o Apache Kafka, para a troca de mensagens assíncronas entre os diferentes componentes da aplicação. Além disso, os eventos de domínio podem ser modelados e integrados à lógica de negócio para processar alterações e propagar informações entre os diferentes serviços.

Lembre-se de que a adoção de uma arquitetura orientada a eventos requer uma análise cuidadosa dos requisitos e uma arquitetura adequada para lidar com a complexidade adicional. É importante considerar fatores como a consistência eventual, a escalabilidade e a confiabilidade do sistema.

Em resumo, esta aplicação de exemplo pode ser adaptada para uma arquitetura orientada a eventos, desde que sejam feitas as modificações e ajustes necessários para atender aos requisitos específicos do projeto.

Divirta-se explorando e desenvolvendo sua aplicação, seja seguindo a abordagem da Clean Architecture ou migrando para uma arquitetura orientada a eventos, conforme necessário.
