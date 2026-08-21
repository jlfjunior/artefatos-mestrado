import { createContext, ReactNode, useCallback, useEffect, useMemo, useState } from 'react'
import { setAuthToken, setUnauthorizedHandler } from '../api/client'
import { login as loginRequest } from '../api/services'
import { Papel } from '../api/types'

interface Sessao {
  token: string
  papel: Papel
  usuario: string
}

interface AuthContextValue {
  sessao: Sessao | null
  autenticado: boolean
  entrar: (usuario: string, senha: string) => Promise<void>
  sair: () => void
}

const STORAGE_KEY = 'fluxo-caixa.sessao'

export const AuthContext = createContext<AuthContextValue | null>(null)

function carregarSessao(): Sessao | null {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (!raw) return null
    const parsed = JSON.parse(raw) as Sessao
    if (parsed.token && parsed.papel) return parsed
    return null
  } catch {
    return null
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [sessao, setSessao] = useState<Sessao | null>(() => carregarSessao())

  // Mantém o token do cliente HTTP sincronizado com a sessão.
  useEffect(() => {
    setAuthToken(sessao?.token ?? null)
  }, [sessao])

  const sair = useCallback(() => {
    setSessao(null)
    localStorage.removeItem(STORAGE_KEY)
    setAuthToken(null)
  }, [])

  // Em qualquer 401 vindo da API, encerra a sessão.
  useEffect(() => {
    setUnauthorizedHandler(sair)
    return () => setUnauthorizedHandler(null)
  }, [sair])

  const entrar = useCallback(async (usuario: string, senha: string) => {
    const resposta = await loginRequest({ usuario, senha })
    const novaSessao: Sessao = {
      token: resposta.token,
      papel: resposta.papel,
      usuario,
    }
    setSessao(novaSessao)
    localStorage.setItem(STORAGE_KEY, JSON.stringify(novaSessao))
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      sessao,
      autenticado: sessao !== null,
      entrar,
      sair,
    }),
    [sessao, entrar, sair],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
