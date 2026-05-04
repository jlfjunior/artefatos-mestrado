# Arquitetura do CashFlow

Esta arquitetura foi pensada para resolver um cenário simples no papel, mas que rapidamente ganha complexidade na prática: registrar lançamentos financeiros com previsibilidade e, ao mesmo tempo, disponibilizar um saldo diário consistente para consulta.

A principal decisão aqui foi separar essas duas responsabilidades em contextos distintos (`Launches` e `Consolidation`) e conectar esses contextos de forma assíncrona. Isso evita acoplamento direto entre escrita e leitura e permite que cada parte evolua no seu próprio ritmo.

---

## Contextos e comunicação

O contexto de `Launches` é responsável por registrar e persistir os lançamentos. Já o contexto de `Consolidation` cuida exclusivamente de transformar esses lançamentos em um saldo diário.

A comunicação entre eles acontece por meio de um contrato explícito de integração (`LaunchRegisteredIntegrationEvent`), publicado no RabbitMQ.

Essa escolha foi intencional: ao usar eventos, o sistema de lançamentos não depende do tempo de processamento da consolidação. Isso garante que o registro de dados continue funcionando mesmo em cenários onde o consolidado esteja indisponível.

---

## Fluxo de dados

O fluxo entre os dois contextos acontece da seguinte forma:

1. O `RegisterLaunchCommandHandler` recebe o comando e persiste o lançamento.
2. Após a execução bem-sucedida do `SaveChangesAsync`, o evento `LaunchRegisteredIntegrationEvent` é publicado no RabbitMQ.
3. O `Consolidation.Worker` consome essa mensagem.
4. O worker dispara o `ProcessLaunchRegisteredEventCommand` via MediatR.
5. O contexto de consolidação atualiza o `DailyConsolidation` e registra o processamento do evento.

Esse fluxo mantém o sistema desacoplado e permite tratar cada etapa de forma independente.

---

## Topologia do RabbitMQ

A comunicação utiliza uma exchange do tipo `topic`, permitindo flexibilidade na evolução de novos consumidores.

* Exchange: `cashflow.launches`
* Queue: `cashflow.consolidation.launch-registered`
* Routing key: `launch.registered`

Essa estrutura facilita a expansão do sistema no futuro, caso outros serviços passem a reagir aos mesmos eventos.

---

## Idempotência

Em sistemas assíncronos, a reentrega de mensagens é um comportamento esperado. Isso significa que o mesmo evento pode ser processado mais de uma vez se não houver controle.

Sem uma estratégia de proteção, isso levaria a inconsistências, como aplicar o mesmo lançamento duas vezes no saldo.

Para evitar esse problema, o contexto de `Consolidation` registra cada lançamento processado em uma tabela (`processed_launch_events`) e aplica uma restrição única baseada no `LaunchId`.

O fluxo considera dois cenários:

* antes de processar, o sistema verifica se o evento já foi aplicado
* em caso de concorrência, a própria restrição única garante que apenas uma execução seja efetiva

Com isso, o processamento se torna seguro mesmo em cenários de retry ou duplicação de mensagens.

---

## Tratamento de falhas

O worker utiliza controle manual de acknowledgments (`autoAck: false`) para garantir maior controle sobre o ciclo de vida das mensagens.

O comportamento adotado foi:

* sucesso: `BasicAck`
* falha de negócio não reprocessável: `BasicAck` com log de warning
* falha inesperada ou transitória: `BasicNack(requeue: true)`

Essa abordagem evita reprocessamentos desnecessários e, ao mesmo tempo, permite recuperação automática em cenários onde a falha é temporária.

---

## Consistência e trade-offs

Atualmente, o evento de integração é publicado apenas após o commit no banco de dados. Isso reduz inconsistências e garante que o dado principal esteja persistido antes de qualquer integração.

Mesmo assim, existe uma janela possível de falha: o commit pode ocorrer com sucesso, mas a publicação no broker pode falhar. Nesse caso, o evento seria perdido.

Essa é uma limitação conhecida do modelo atual e foi uma escolha consciente para manter a solução simples dentro do escopo do desafio.

---

## Evolução com Outbox Pattern

Como evolução natural, o sistema pode adotar o Outbox Pattern para eliminar essa lacuna de consistência.

A ideia é simples:

1. salvar o lançamento e o evento na mesma transação de banco
2. um processo em background lê os eventos pendentes
3. esse processo publica no RabbitMQ e marca como processado

Com isso, a publicação passa a ser confiável mesmo em cenários de falha entre banco e broker, sem necessidade de mudanças no modelo de domínio ou nos contratos de integração.

---

## Considerações finais

A arquitetura foi desenhada com foco em separação de responsabilidades, resiliência e facilidade de evolução.

Mesmo sendo uma solução simples, ela já incorpora conceitos importantes como comunicação assíncrona, idempotência e desacoplamento entre contextos, permitindo que o sistema cresça sem comprometer o fluxo principal de escrita.
