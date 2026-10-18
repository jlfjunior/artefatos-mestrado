import { consolidadoApi, lancamentosApi } from './client'
import {
  Consolidado,
  LancamentoRequest,
  LancamentoResponse,
  LoginRequest,
  LoginResponse,
  RelatorioConsolidado,
} from './types'

export function login(body: LoginRequest): Promise<LoginResponse> {
  return lancamentosApi.post<LoginResponse>('/token', body)
}

export function registrarLancamento(body: LancamentoRequest): Promise<LancamentoResponse> {
  return lancamentosApi.post<LancamentoResponse>('/lancamentos', body, { auth: true })
}

export function consultarConsolidado(data: string): Promise<Consolidado> {
  return consolidadoApi.get<Consolidado>(`/consolidado/${data}`, { auth: true })
}

export function consultarRelatorio(de: string, ate: string): Promise<RelatorioConsolidado> {
  const query = new URLSearchParams({ de, ate }).toString()
  return consolidadoApi.get<RelatorioConsolidado>(`/consolidado/relatorio?${query}`, { auth: true })
}
