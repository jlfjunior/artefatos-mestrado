import { ApiError, ValidationErrors } from './types'

const LANCAMENTOS_URL = import.meta.env.VITE_LANCAMENTOS_URL ?? 'http://localhost:5080'
const CONSOLIDADO_URL = import.meta.env.VITE_CONSOLIDADO_URL ?? 'http://localhost:5090'

export { LANCAMENTOS_URL, CONSOLIDADO_URL }

// Token corrente em memória. Mantido em sincronia com o AuthContext.
let authToken: string | null = null

export function setAuthToken(token: string | null) {
  authToken = token
}

// Disparado quando a API responde 401 (sessão expirada/inválida).
// O AuthContext registra um handler para deslogar o usuário.
let onUnauthorized: (() => void) | null = null

export function setUnauthorizedHandler(handler: (() => void) | null) {
  onUnauthorized = handler
}

interface RequestOptions {
  method?: string
  body?: unknown
  auth?: boolean
}

async function request<T>(baseUrl: string, path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, auth = false } = options

  const headers: Record<string, string> = {}
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }
  if (auth && authToken) {
    headers['Authorization'] = `Bearer ${authToken}`
  }

  let response: Response
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    })
  } catch {
    // Falha de rede / servidor fora do ar.
    throw new ApiError(0, 'Não foi possível conectar ao servidor. Verifique se a API está no ar.')
  }

  if (response.status === 401) {
    onUnauthorized?.()
    throw new ApiError(401, 'Sessão expirada ou credenciais inválidas.')
  }

  if (response.status === 403) {
    throw new ApiError(403, 'Acesso negado: seu perfil não tem permissão para esta operação.')
  }

  if (response.ok) {
    if (response.status === 204) {
      return undefined as T
    }
    return (await response.json()) as T
  }

  // Tenta extrair detalhes do corpo de erro.
  const payload = await safeJson(response)

  if (response.status === 400 && payload && typeof payload === 'object' && 'errors' in payload) {
    const errors = (payload as { errors: ValidationErrors }).errors
    throw new ApiError(400, 'Há campos inválidos no formulário.', errors)
  }

  const detail =
    (payload && typeof payload === 'object' && 'detail' in payload
      ? String((payload as { detail: unknown }).detail)
      : null) ?? `Erro inesperado (HTTP ${response.status}).`

  throw new ApiError(response.status, detail)
}

async function safeJson(response: Response): Promise<unknown> {
  try {
    return await response.json()
  } catch {
    return null
  }
}

export const lancamentosApi = {
  get: <T>(path: string, opts?: RequestOptions) => request<T>(LANCAMENTOS_URL, path, opts),
  post: <T>(path: string, body: unknown, opts?: RequestOptions) =>
    request<T>(LANCAMENTOS_URL, path, { ...opts, method: 'POST', body }),
}

export const consolidadoApi = {
  get: <T>(path: string, opts?: RequestOptions) => request<T>(CONSOLIDADO_URL, path, opts),
}
