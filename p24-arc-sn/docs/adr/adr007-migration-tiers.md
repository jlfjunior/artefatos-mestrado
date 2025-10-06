# Architecture Decision Record 007 - Migration Tiers

Para simplificar a priorização dos scripts repetitivos do DB Migration, os scripts de stored procedure deverão iniciar
do tier 0 (`T00`) e os scripts de dataset deverão derivar do tier 99 (`T99`), garantindo que após os scripts versionados
serão executados os tiers em ordem de dependência, permitindo a adoção de execução de procedures nos scripts de dataset.
