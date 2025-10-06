# Architecture Decision Record 013 - Setup

Para permitir adicionar serviços do contexto startup foi adotada como estratégio um método de extensão `Setup`,
para concentrar a construção do que for inerentemente acoplado ao contexto web, como foi efetuado no `Configure`.
O `Setup` passa a ser o responsável por acionar o `AddCompositionRoot`.
