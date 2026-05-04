# Arquitetura da Implementação

## Objetivo

Materializar em código uma solução simples, resiliente e fácil de executar localmente, sem abrir mão dos princípios arquiteturais pedidos no desafio.

## Serviços

### 1. CashFlow.Lancamentos.Api

Responsabilidades:

- validar entrada;
- aplicar idempotência;
- persistir a razão de lançamentos;
- registrar o evento de integração na saída transacional;
- publicar eventos pendentes no barramento local.

### 2. CashFlow.Consolidado.Api

Responsabilidades:

- consumir eventos publicados;
- projetar saldo diário por estabelecimento e data;
- expor consulta de saldo diário;
- permitir reprocessamento de uma data específica.

## Persistência

### lancamentos.db

- estrutura principal da razão de lançamentos gravados.
- estrutura da saída transacional com eventos prontos para publicação assíncrona.

### integration.db

- fila local durável entre os dois serviços.

### consolidado.db

- saldo diário materializado.
- controle de eventos já aplicados.
- último ponto de processamento do consumidor.

## Justificativa técnica

Foi escolhida uma implementação local com SQLite por três motivos:

1. Permite rodar o projeto sem dependências externas.
2. Mantém persistência durável para demonstrar saída transacional, fila e projeção.
3. Preserva o desenho para evolução posterior para banco e mensageria de produção.

## Fluxo de falha mais importante

Se o `CashFlow.Consolidado.Api` cair:

- o `CashFlow.Lancamentos.Api` continua registrando lançamentos;
- os eventos seguem sendo persistidos;
- as mensagens ficam acumuladas em `integration.db`;
- quando o consolidado volta, o processo em segundo plano consome os eventos pendentes e recompõe a projeção.

Esse é o ponto central de resiliência pedido pelo desafio.

## Escalabilidade

Para o escopo local, a solução atende funcionalmente ao desenho requerido. Para produção, o caminho natural de escala seria:

- múltiplas instâncias do serviço de lançamentos atrás de um balanceador de carga;
- banco transacional gerenciado;
- infraestrutura dedicada de mensageria com partições ou filas;
- serviço de consolidado escalando consumidores conforme o volume acumulado;
- cache para leituras mais frequentes, se necessário.
