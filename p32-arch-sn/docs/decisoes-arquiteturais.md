# Decisões Arquiteturais

## ADR-001 - Separar gravação e leitura em serviços distintos

### Decisão

Implementar dois serviços separados, um para lançamentos e outro para saldo consolidado.

### Motivo

Atender ao requisito de independência operacional entre os domínios.

## ADR-002 - Usar saída transacional

### Decisão

Persistir o lançamento e o evento de integração na mesma transação local.

### Motivo

Evitar o risco de gravar o lançamento e perder o evento, ou publicar evento sem persistir a operação de negócio.

## ADR-003 - Materializar o saldo diário

### Decisão

Consultar o consolidado a partir de uma projeção pronta, e não recalcular tudo a cada leitura.

### Motivo

Reduzir latência, atender melhor o pico de leitura e manter o domínio de leitura desacoplado.

## ADR-004 - Implementar idempotência explícita

### Decisão

Exigir `Chave-Idempotencia` como contrato principal da API e armazenar o hash do payload original.

### Motivo

Garantir que novas tentativas de rede não dupliquem o efeito financeiro e detectar reaproveitamento indevido da mesma chave com outro conteúdo.

## ADR-005 - Usar SQLite como infraestrutura local de demonstração

### Decisão

Modelar a persistência e o barramento local com SQLite.

### Motivo

Manter a solução autocontida para execução local, sem abrir mão do desenho arquitetural necessário para evoluir para produção.
