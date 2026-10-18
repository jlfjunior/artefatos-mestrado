import { Papel } from '../api/types'

// Mapa central de quem acessa o quê. Usado tanto pelo menu quanto pelos guards.
export const PAPEIS_LANCAMENTOS: Papel[] = ['Operador', 'Admin']
export const PAPEIS_CONSOLIDADO: Papel[] = ['Gerente', 'Admin']

export function podeAcessar(papel: Papel | undefined, papeisPermitidos: Papel[]): boolean {
  return papel !== undefined && papeisPermitidos.includes(papel)
}
