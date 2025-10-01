# Title

Uso do padrão /me/ em endpoints autenticados

## Status

Implementada

## Context

A autenticação deve garantir que o usuário tenha acesso apenas as suas próprias informações e não de demais usuário em hipotese alguma

## Decision

A adoção do padrão /me/ para endpoints que retornam ou manipulam dados do usuário autenticado:

O endpoint infere o usuário a partir do token de autenticação (JWT).

Exemplos:
GET /me/accounts → retorna contas do usuário
GET /me/transactions → retorna transações do usuário logado

Com isso não é necessário enviar o ID do usuário na URL ou no corpo da requisição.

## Consequences
Para operações administrativas ou de terceiros, será necessário usar endpoints espeficicos como /admin/.