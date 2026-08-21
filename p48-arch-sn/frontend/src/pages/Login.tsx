import { FormEvent, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/types'
import { useAuth } from '../auth/useAuth'

export function Login() {
  const { entrar } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [usuario, setUsuario] = useState('')
  const [senha, setSenha] = useState('')
  const [erro, setErro] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  const destino = (location.state as { from?: string } | null)?.from ?? '/'

  async function aoEnviar(e: FormEvent) {
    e.preventDefault()
    setErro(null)
    setEnviando(true)
    try {
      await entrar(usuario.trim(), senha)
      navigate(destino, { replace: true })
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        setErro('Usuário ou senha inválidos.')
      } else if (err instanceof ApiError) {
        setErro(err.message)
      } else {
        setErro('Não foi possível efetuar o login.')
      }
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="login-tela">
      <form className="login-card" onSubmit={aoEnviar}>
        <h1 className="login-titulo">Fluxo de Caixa</h1>
        <p className="login-sub">Acesse com suas credenciais</p>

        {erro && <div className="alerta alerta-erro">{erro}</div>}

        <div className="campo">
          <label htmlFor="usuario">Usuário</label>
          <input
            id="usuario"
            type="text"
            autoComplete="username"
            value={usuario}
            onChange={(e) => setUsuario(e.target.value)}
            required
          />
        </div>

        <div className="campo">
          <label htmlFor="senha">Senha</label>
          <input
            id="senha"
            type="password"
            autoComplete="current-password"
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            required
          />
        </div>

        <button type="submit" className="botao-primario" disabled={enviando}>
          {enviando ? 'Entrando…' : 'Entrar'}
        </button>
      </form>
    </div>
  )
}
