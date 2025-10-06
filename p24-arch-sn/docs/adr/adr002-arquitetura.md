# Architecture Decision Record 001 - Arquitetura

Será utilizado uma aplicação web para a interface e pricipais capabilities da solução, banco de dados relaciona,
um provedor de cache, uma mensageria e um worker para processos em segundo plano.
Num primeiro momento a intenção é subir containers numa mesma rede virtual do host expondo apenas a aplicação web
através do host.
