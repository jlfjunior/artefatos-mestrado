# Fluxo de Caixa — Frontend

Interface web para o sistema de Controle de Fluxo de Caixa Diário. Permite registrar
lançamentos (créditos e débitos) e consultar o consolidado diário, com controle de
acesso por papel (Operador, Gerente, Admin).

A aplicação consome duas APIs do backend:

- **Lançamentos** (`/token`, `/lancamentos`)
- **Consolidado** (`/consolidado/{data}`)

## Funcionalidades

- **Login** com usuário e senha. O token e o papel ficam em memória e no `localStorage`,
  para sobreviver a um refresh da página.
- **Lançamentos** (Operador e Admin): formulário com tipo, valor, data (padrão: hoje) e
  descrição opcional. Exibe os erros de validação retornados pela API (400) e os erros de
  invariante de domínio (422), além de confirmar o sucesso com o ID gerado.
- **Consolidado** (Gerente e Admin): seleção de data e exibição de saldo, total de créditos
  e total de débitos em cards. Trata o caso de não haver lançamentos no dia.
- **Controle de acesso em camadas**:
  - O menu mostra apenas o que o papel pode acessar.
  - As rotas são protegidas: sem sessão, redireciona ao login; sem o papel necessário,
    exibe "Acesso negado".
  - As respostas da API são interceptadas: `401` desloga e volta ao login; `403` exibe
    mensagem de acesso negado.
- **Logout** limpando o token.

## Como rodar

Pré-requisito: Node.js 18+.

```bash
npm install
npm run dev
```

A aplicação sobe em http://localhost:5173.

Para gerar a build de produção:

```bash
npm run build
npm run preview
```

## Variáveis de ambiente

Copie o `.env.example` para `.env` e ajuste as URLs se necessário:

```
VITE_LANCAMENTOS_URL=http://localhost:5080
VITE_CONSOLIDADO_URL=http://localhost:5090
```

| Variável                | Descrição                          | Padrão                  |
| ----------------------- | ---------------------------------- | ----------------------- |
| `VITE_LANCAMENTOS_URL`  | URL base da API de lançamentos     | `http://localhost:5080` |
| `VITE_CONSOLIDADO_URL`  | URL base da API de consolidado     | `http://localhost:5090` |

## Usuários de teste

A senha é a mesma para os três (`Senha@123`):

| Usuário    | Papel    | Acessa                      |
| ---------- | -------- | --------------------------- |
| `operador` | Operador | Lançamentos                 |
| `gerente`  | Gerente  | Consolidado                 |
| `admin`    | Admin    | Lançamentos e Consolidado   |

## Estrutura

```
src/
  api/         clientes HTTP e tipos dos contratos da API
  auth/        contexto de autenticação, hook useAuth, guard de rota e permissões
  components/  layout, navegação e campo de formulário reutilizável
  pages/       Login, Lançamentos e Consolidado
  utils/       formatação de moeda e data
```

## Stack

React + Vite + TypeScript, React Router para roteamento e Context API para o estado de
autenticação. O cliente HTTP usa `fetch` nativo encapsulado, com tratamento centralizado
dos códigos de erro da API.
