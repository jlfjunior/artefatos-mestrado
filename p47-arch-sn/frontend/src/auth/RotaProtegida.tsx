import { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { Papel } from '../api/types'
import { podeAcessar } from './permissoes'
import { useAuth } from './useAuth'

interface Props {
  children: ReactNode
  papeisPermitidos?: Papel[]
}

/**
 * Protege uma rota. Sem sessão, manda para o login. Com sessão mas sem o
 * papel exigido, mostra "Acesso negado" em vez de quebrar a aplicação.
 */
export function RotaProtegida({ children, papeisPermitidos }: Props) {
  const { sessao, autenticado } = useAuth()

  if (!autenticado) {
    return <Navigate to="/login" replace />
  }

  if (papeisPermitidos && !podeAcessar(sessao?.papel, papeisPermitidos)) {
    return (
      <div className="acesso-negado">
        <h2>Acesso negado</h2>
        <p>Seu perfil ({sessao?.papel}) não tem permissão para acessar esta página.</p>
      </div>
    )
  }

  return <>{children}</>
}
