# Architecture Decision Record 008 - Cross Cutting Concerns

Para ampliar o isolamento e desacoplamento dos contextos da solução foi adicionada a camada transversal CrossCutting.
Toda a amarração entre os contextos é feita na Cross Cutting tendo como arbitração a camada de domínio.

Para evitar referência cíclica, o método de extensão `WebApplication Configure(this WebApplication app)` na camada web
fica responsável por construir e configurar o contexto web quanto a artefatos do próprio contexto.
