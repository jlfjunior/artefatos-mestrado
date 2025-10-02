# ADR 004 – Keycloak

**Status**: Aprovado  
**Data**: 29/05/2025

## Contexto

A plataforma Cashflow App, composta por microsserviços desenvolvidos em .NET, necessita de autenticação e autorização com suporte a múltiplos usuários e roles. Isso vai garantir segurança.

## Decisão

Ficou decidido utilizar o Keycloak como provedor de identidade (IDP) para gerenciar autenticação e autorização. A implantação será feita em cluster Kubernetes como serviço.

## Justificativas

- **Autenticação Centralizada**: Permite controle dos usuários, tokens e roles.
- **Padrões de segurança**: Compatível com OpenID Connect, OAuth2, JWT, garantindo segurança e flexibilidade.
- **Administração de usuários**: UI de gestão para user, roles e grupos.

## Consequências

- A gestão de roles passa a ser gerenciada via UI Keycloak.

## Alternativas

- **Auth0**: Solução SaaS robusta, mas com alto custo.
- **IdentityServer**: Boa alternativa de server Identity, mas com complexidade de configuração e custos de licenciamento.

## Conclusão

Decisão validada com foco em segurança e gestão independente de usuários.
