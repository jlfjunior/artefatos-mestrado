# ADR-016 – OAuth2, JWT e autorização por escopos

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

O sistema expõe API REST e interface Blazor. É necessário autenticar usuários (funcionário e cliente) e restringir operações por perfil — por exemplo, cliente só consulta os próprios lançamentos e saldos.

---

## Decisão

Implementar **OAuth2** com grant type `password` e tokens **JWT** assinados com chaves rotacionáveis:

1. **`POST /oauth/token`** — emissão de access token
2. **`/.well-known/openid-configuration`** e **`/.well-known/jwks.json`** — descoberta OIDC
3. **Escopos** por operação (`transactions.write`, `balances.read.self`, etc.)
4. **Políticas ASP.NET Core** mapeando escopos para controllers
5. **Blazor Web** obtém token via client confidencial `cashflow.web` e envia `Authorization: Bearer`

Usuários seed (funcionário e cliente) são criados na migration `AuthAndClientOwnership`.

---

## Justificativa

1. Padrão amplamente adotado para APIs e SPAs/Blazor.
2. Escopos permitem evoluir autorização sem acoplar à role apenas.
3. JWKS permite validação de tokens sem compartilhar segredo simétrico com consumidores externos.

---

## Consequências Positivas

- API protegida por padrão (`[Authorize]` nos controllers de negócio)
- Separação clara entre autenticação (Infrastructure/Auth) e regras de acesso (Application)

---

## Consequências Negativas

- Grant `password` é desaconselhado em produção (preferir Authorization Code + PKCE)
- Rotação de chaves exige coordenação entre instâncias (pool de chaves em configuração)

---

## Alternativas Consideradas

1. **API Key estática** — Rejeitada por falta de escopos e expiração.
2. **Autenticação apenas no Blazor (cookie)** — Rejeitada; API precisa ser consumível de forma independente.

---

## Referências

- [c4-definicoes-fluxo-caixa.md](../c4-definicoes-fluxo-caixa.md)
- RFC 6749 (OAuth 2.0), RFC 7519 (JWT)
