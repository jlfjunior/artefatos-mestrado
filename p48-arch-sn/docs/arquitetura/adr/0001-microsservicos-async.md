# ADR 0001 — Dois microsserviços com comunicação assíncrona

Status: aceito

## Contexto
O requisito não-funcional mais forte do problema é claro: o serviço de
lançamentos não pode ficar indisponível se o consolidado cair. Em pico, o
consolidado recebe 50 req/s e tolera até 5% de perda. Ou seja, os dois lados
têm perfis de disponibilidade e carga diferentes e não podem compartilhar
destino.

Se eu mantivesse tudo num processo só, uma sobrecarga ou crash do consolidado
derrubaria a escrita junto. Uma chamada síncrona de um para o outro teria o
mesmo efeito: o lançamento passaria a depender da saúde do consolidado.

## Decisão
Separar em dois serviços deployáveis de forma independente — Lançamentos
(escrita) e Consolidado (leitura) — e fazer a comunicação entre eles ser
**100% assíncrona** via RabbitMQ. Lançamentos publica eventos; Consolidado
consome no seu ritmo. Nenhuma chamada síncrona entre os serviços.

## Consequências
- Lançamentos continua aceitando registros mesmo com o consolidado fora: os
  eventos se acumulam na fila e são processados quando ele volta.
- O sistema passa a ser eventualmente consistente. A folga de 5% de perda
  prevista no enunciado deixa isso confortável — o saldo consolidado pode
  ficar alguns instantes atrás da escrita.
- Ganho de resiliência ao custo de complexidade operacional: agora há um broker
  para operar e monitorar. Para o tamanho do domínio, dois serviços é o ponto
  certo; mais do que isso seria over-engineering.
