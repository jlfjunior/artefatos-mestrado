# ADR-012 – Convenções de idioma (código vs documentação)

Data: 2026-06-13  
Status: Aprovado  
Responsáveis: Samuel Fabel

---

## Contexto

Requisitos conflitantes aparentes: código e rotas em inglês; documentação em português do Brasil.

---

## Decisão

| Artefato                                    | Idioma             |
| ------------------------------------------- | ------------------ |
| Código (classes, métodos, variáveis, enums) | Inglês             |
| Rotas HTTP (`/api/transactions`, etc.)      | Inglês             |
| Mensagens de evento (tipo, routing key)     | Inglês             |
| Comentários XML/docstrings no código        | Português (Brasil) |
| ADRs, C4, README, docs/                     | Português (Brasil) |
| Textos de UI Blazor                         | Português (Brasil) |

---

## Justificativa

1. Inglês no código facilita integração, logs e convenções REST globais.
2. Documentação em PT-BR atende avaliadores e time local.
3. Alinhado ao estilo origami-authentication para docs/ADRs.

---

## Consequências Positivas

- Consistência clara por tipo de artefato
- APIs consumíveis por ferramentas padrão

---

## Consequências Negativas

- Desenvolvedores devem alternar idioma conforme camada
- Mensagens de erro de API podem ser bilingues (evolução)

---

## Alternativas Consideradas

1. **Tudo em inglês** — Rejeitado por requisito de documentação PT-BR.
2. **Tudo em português** — Rejeitado por requisito de código/rotas em inglês.
