# Architecture Decision Record 006 - Database Setup

Devido a desafios apresentados pelo controle de transação do DB Migration, serão utilizadas 3 etapas de migration.

1. Criação do banco de dados `BCCF` a partir do contexto do banco `master` e usuário `sa`.
2. Criação do usuário `usr_bccf` a partir do contexto `BCCF` e usuário `sa`.
3. Aplicação o migration da aplicação a partir do contexto `BCCF` e usuário `usr_bccf`.
