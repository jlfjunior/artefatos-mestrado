# Decisões de arquitetura

Este documento registra as decisões tomadas para o projeto de controle de
fluxo de caixa e o racional por trás de cada uma. Serve tanto como
documentação do projeto quanto como referência para a implementação.

## Contexto

Um comerciante precisa registrar lançamentos diários (débito e crédito) e
consultar o saldo diário consolidado. O domínio é simples de propósito; o
desafio está em atender os requisitos não-funcionais, em especial:

> O serviço de controle de lançamento não deve ficar indisponível se o
> sistema de consolidado diário cair. Em dias de pico o consolidado recebe
> 50 req/s, com no máximo 5% de perda de requisições.

Essa restrição é o que dita todo o desenho: os dois serviços precisam ser
independentes em disponibilidade.

## Decisões travadas

### 1. Dois microsserviços
Lançamentos e Consolidado são serviços deployáveis separadamente. Um monólito
não atenderia o requisito de "um cair sem derrubar o outro". Mais do que dois
serviços seria over-engineering para o tamanho do domínio.

### 2. CQRS
- **Lançamentos** é o write model: fonte da verdade, escrita transacional.
- **Consolidado** é o read model: projeção do saldo por dia, otimizada para
  leitura e cacheável.

### 3. Comunicação assíncrona via RabbitMQ
Lançamentos publica eventos; Consolidado consome e atualiza a projeção. Não há
chamada síncrona entre eles. Se o Consolidado cair, os eventos se acumulam na
fila e o Lançamentos segue operando. Exchange do tipo *topic* para permitir
novos consumidores no futuro sem alterar o produtor.

### 4. Transactional Outbox
Para evitar o *dual-write problem* (gravar no banco e publicar na fila como
operações separadas que podem falhar isoladamente), o evento é gravado numa
tabela de outbox na **mesma transação** do lançamento. Um processo publica a
partir da outbox. Com MassTransit isso é nativo.

### 5. Consumidor idempotente
A entrega *at-least-once* do broker pode reentregar a mesma mensagem. O
consumidor do Consolidado usa o `LancamentoId` como chave de idempotência para
não dobrar o saldo. Mensagens que falham repetidamente vão para uma DLQ.

### 6. Stack
- **.NET 8 (LTS)** com **Minimal API**.
- **Arquitetura hexagonal** por serviço: `Domain` (núcleo puro), `Application`
  (casos de uso / portas), `Infrastructure` (adapters: EF Core, MassTransit,
  Outbox), `Api` (adapter dirigente).
- **MassTransit** sobre RabbitMQ — abstrai mensageria e entrega Outbox, retry e
  DLQ prontos.
- **EF Core** para persistência.
- **FluentValidation** para validação na borda.
- **Serilog** para log estruturado.
- **MediatR deliberadamente fora**: na hexagonal os casos de uso já são
  application services expostos por portas; empilhar MediatR seria indireção
  desnecessária.
- Testes: **xUnit + FluentAssertions + Testcontainers** (RabbitMQ e banco reais
  nos testes de integração).

### 7. Estrutura da solution (repositório e solution únicos)

```
fluxo-caixa-diario/
├── docs/
├── src/
│   ├── Lancamentos/
│   │   ├── Lancamentos.Domain/
│   │   ├── Lancamentos.Application/
│   │   ├── Lancamentos.Infrastructure/
│   │   └── Lancamentos.Api/
│   ├── Consolidado/
│   │   ├── Consolidado.Domain/
│   │   ├── Consolidado.Application/
│   │   ├── Consolidado.Infrastructure/
│   │   └── Consolidado.Api/
│   └── BuildingBlocks/
│       └── Contracts/          # eventos de integração compartilhados
├── tests/
│   ├── Lancamentos.UnitTests/
│   ├── Lancamentos.IntegrationTests/
│   ├── Consolidado.UnitTests/
│   └── Consolidado.IntegrationTests/
├── docker-compose.yml
├── global.json
└── FluxoCaixa.sln
```

O projeto `Contracts` compartilha o schema dos eventos entre produtor e
consumidor. Acopla levemente os serviços, mas evita divergência de schema —
trade-off aceitável neste escopo.

### 8. SDK / global.json
O único SDK da linha 8 disponível na máquina é um preview
(`8.0.100-preview.2`). Para entregar em .NET 8 LTS sem depender do preview,
fixar um `global.json` na raiz usando o SDK final mais novo (10.0.201) com
`rollForward: latestMajor`, mantendo `<TargetFramework>net8.0</TargetFramework>`
nos projetos. O SDK 10 compila targets net8.0 normalmente.

```json
{
  "sdk": {
    "version": "10.0.201",
    "rollForward": "latestMajor"
  }
}
```

## Requisitos não-funcionais — como são atendidos

| RNF | Solução |
|---|---|
| Lançamento disponível mesmo com Consolidado fora | Async total, bancos separados, fila bufferiza |
| 50 req/s no Consolidado, até 5% de perda | Read model + cache + consumidor escalável; folga de 5% permite consistência eventual |
| Integridade do saldo | Outbox (atomicidade) + consumidor idempotente |

## Decisões complementares

- **Banco de dados**: PostgreSQL nos dois serviços. Open-source, sobe fácil no
  Docker Compose e tem ótimo suporte no EF Core.
- **Cache de leitura**: Redis no Consolidado. Cache distribuído real, alinhado
  com a meta de 50 req/s; sobe junto no Compose.
- **Segurança**: autenticação JWT Bearer nas APIs (código funcional). O desenho
  completo (OAuth2/Identity Provider, API Gateway, mTLS entre serviços) fica
  descrito na documentação como evolução.
- **Observabilidade**: health checks, Serilog (log estruturado) e OpenTelemetry
  (tracing + métricas) desde o MVP, exportando via OTLP.
