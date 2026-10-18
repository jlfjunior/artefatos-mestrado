# ADR 0005 — Não usar MediatR

Status: aceito

## Contexto
É comum em projetos .NET com arquitetura limpa empilhar MediatR e mandar todo
caso de uso por um `IMediator.Send`. A pergunta é se isso agrega aqui.

Na arquitetura hexagonal que adotei, o caso de uso **já é** um objeto de
primeira classe: um application service com um método explícito
(`RegistrarLancamentoService.ExecutarAsync`, `ConsultarSaldoService.ConsultarAsync`).
A API chama esse serviço direto, injetado por DI. As portas (interfaces de
repositório, publisher, cache) já dão a inversão de dependência.

## Decisão
**Não** adotar MediatR. Os casos de uso ficam como application services
explícitos, expostos e chamados diretamente.

## Consequências
- Menos uma indireção e uma dependência. O fluxo do request é lível de ler: do
  endpoint para o serviço, sem um dispatcher no meio.
- Navegação direta no código — "vá para a definição" do método cai no caso de
  uso, não num handler resolvido por tipo em runtime.
- Abro mão dos behaviors de pipeline do MediatR (logging, validação central).
  Não sinto falta: validação fica na borda com FluentValidation, e
  logging/tracing vêm de Serilog + OpenTelemetry de forma transversal.
- Se um dia o número de casos de uso explodir e o pipeline cross-cutting virar
  dor, dá para reavaliar. No escopo atual, MediatR seria cerimônia sem retorno.
