# Architecture Decision Record 014 - Dockerfile

A adoção do EvolveDB como Docker estava provocando o uso de diversos containers com pouca duração de execução e com a
necessidade de retentativas na execução por depender de serviços e disponibilidades externas. Para reduzir este problema
e evitar a execução incompleta, foi criado um Dockerfile cobrindo a verificação da disponibilidade dos serviços, tempo
adicional de espera, execução dos migrations e inicialização das aplicações (Web e Scheduler).
