export type Papel = 'Operador' | 'Gerente' | 'Admin'

export type TipoLancamento = 'Credito' | 'Debito'

export interface LoginRequest {
  usuario: string
  senha: string
}

export interface LoginResponse {
  token: string
  papel: Papel
  expiraEmHoras: number
}

export interface LancamentoRequest {
  tipo: TipoLancamento
  valor: number
  data: string // yyyy-MM-dd
  descricao: string | null
}

export interface LancamentoResponse {
  id: string
}

export interface Consolidado {
  data: string
  totalCreditos: number
  totalDebitos: number
  saldo: number
  atualizadoEmUtc?: string
  mensagem?: string
}

export interface RelatorioConsolidado {
  de: string
  ate: string
  dias: Consolidado[]
  totalCreditos: number
  totalDebitos: number
  saldo: number
}

/** Erros de validação (400) no formato ValidationProblem do ASP.NET. */
export type ValidationErrors = Record<string, string[]>

/**
 * Erro normalizado lançado pelos clientes da API. O componente decide
 * como exibir conforme o status (validação por campo, invariante, etc).
 */
export class ApiError extends Error {
  status: number
  validationErrors?: ValidationErrors

  constructor(status: number, message: string, validationErrors?: ValidationErrors) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.validationErrors = validationErrors
  }
}
