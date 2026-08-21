import { ReactNode } from 'react'

interface Props {
  label: string
  htmlFor?: string
  erro?: string
  children: ReactNode
}

/** Wrapper de campo de formulário: label + controle + mensagem de erro. */
export function Campo({ label, htmlFor, erro, children }: Props) {
  return (
    <div className={erro ? 'campo campo-erro' : 'campo'}>
      <label htmlFor={htmlFor}>{label}</label>
      {children}
      {erro && <span className="mensagem-erro-campo">{erro}</span>}
    </div>
  )
}
