# Architecture Decision Record 011 - Startup Context Builder

Modificado o `AddCompositionRoot(this WebApplicationBuilder builder)` para receber o tipo do startup context builder,
`TStartupContextBuilder`, permitindo o compartilhamento do Cross Cutting entre múltiplos startups.
