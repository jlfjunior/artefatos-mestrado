# Revisão para Banca Técnica

## Como defender a solução

O ponto principal da entrega é a independência operacional entre gravação e leitura. O serviço de lançamentos aceita a operação, grava o lançamento e registra o evento de integração na mesma transação. O consolidado consome depois, de forma assíncrona, sem bloquear a entrada de novas movimentações.

## Pontos fortes

- Separação clara entre escrita de lançamentos e leitura de saldo consolidado.
- Idempotência explícita para evitar duplicidade financeira em reenvios.
- Persistência local durável, suficiente para demonstrar resiliência sem depender de infraestrutura externa.
- Projeção materializada para responder consultas com baixa latência.
- Reprocessamento por data para recompor o consolidado quando necessário.
- Testes cobrindo regras críticas do domínio e do fluxo assíncrono.

## Trade-offs assumidos

- `SQLite` foi escolhido para simplificar a execução local, não para representar a infraestrutura final de produção.
- A fila local em `integration.db` facilita a demonstração, mas em produção o ideal seria usar uma tecnologia de mensageria dedicada.
- A consistência entre lançamento e consolidado é eventual, não imediata.
- O consolidado foi otimizado para leitura, então ele depende do fluxo assíncrono estar saudável para se manter atualizado.

## Perguntas prováveis e respostas curtas

### Por que separar em dois serviços?

Porque o desafio exige que o lançamento continue disponível mesmo se o consolidado estiver indisponível. Separar escrita e leitura reduz acoplamento e isola falhas.

### Como você garante que um lançamento não se perde?

O lançamento e o evento de integração são gravados na mesma transação local. Se a transação confirma, os dois existem. Se falha, nenhum dos dois é persistido.

### Como evita duplicidade financeira?

Com a `Chave-Idempotencia` associada ao comerciante e ao hash do payload original. Se a mesma chave chegar com o mesmo conteúdo, o sistema responde como reenvio. Se chegar com conteúdo diferente, retorna conflito.

### O que acontece se o consolidado cair?

Os lançamentos continuam sendo aceitos. Os eventos ficam acumulados na fila local durável e são consumidos quando o consolidado volta.

### Por que usar projeção materializada?

Porque consultar saldo diário é cenário de leitura frequente. Manter o saldo pré-calculado reduz latência e evita recalcular todo o histórico a cada consulta.

### Como evoluir isso para produção?

Substituindo `SQLite` por banco transacional gerenciado, a fila local por mensageria dedicada, adicionando observabilidade, autenticação, escalabilidade horizontal e políticas de retentativa.

## Riscos conhecidos

- `SQLite` não é a melhor escolha para alta concorrência.
- Ainda não há autenticação, autorização e trilha operacional completa.
- O mecanismo atual depende de processos em segundo plano estarem ativos para reduzir o atraso do consolidado.
- Não há fila de exceção nem estratégia avançada de retentativa para falhas persistentes.

## Melhorias que eu priorizaria em seguida

- instrumentação com métricas, rastreamento e alarmes;
- troca da fila local por mensageria dedicada;
- autenticação e autorização;
- testes de integração ponta a ponta;
- empacotamento com contêineres e execução orquestrada.
