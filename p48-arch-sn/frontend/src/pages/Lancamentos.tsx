import { FormEvent, useState } from 'react'
import { registrarLancamento } from '../api/services'
import { ApiError, TipoLancamento, ValidationErrors } from '../api/types'
import { Campo } from '../components/Campo'
import { hojeIso } from '../utils/formato'

export function Lancamentos() {
  const [tipo, setTipo] = useState<TipoLancamento>('Credito')
  const [valor, setValor] = useState('')
  const [data, setData] = useState(hojeIso())
  const [descricao, setDescricao] = useState('')

  const [errosCampo, setErrosCampo] = useState<ValidationErrors>({})
  const [erroGeral, setErroGeral] = useState<string | null>(null)
  const [sucessoId, setSucessoId] = useState<string | null>(null)
  const [enviando, setEnviando] = useState(false)

  function limparMensagens() {
    setErrosCampo({})
    setErroGeral(null)
    setSucessoId(null)
  }

  async function aoEnviar(e: FormEvent) {
    e.preventDefault()
    limparMensagens()

    const valorNumerico = Number(valor.replace(',', '.'))
    if (!valor || Number.isNaN(valorNumerico)) {
      setErrosCampo({ Valor: ['Informe um valor numérico válido.'] })
      return
    }

    setEnviando(true)
    try {
      const resposta = await registrarLancamento({
        tipo,
        valor: valorNumerico,
        data,
        descricao: descricao.trim() === '' ? null : descricao.trim(),
      })
      setSucessoId(resposta.id)
      // Limpa apenas valor e descrição, mantendo tipo e data para lançamentos em sequência.
      setValor('')
      setDescricao('')
    } catch (err) {
      if (err instanceof ApiError && err.status === 400 && err.validationErrors) {
        setErrosCampo(err.validationErrors)
      } else if (err instanceof ApiError) {
        // 422 (invariante de domínio) e demais erros.
        setErroGeral(err.message)
      } else {
        setErroGeral('Não foi possível registrar o lançamento.')
      }
    } finally {
      setEnviando(false)
    }
  }

  // Os campos vêm com a primeira letra maiúscula no dicionário do backend.
  const erro = (campo: string) => errosCampo[campo]?.[0]

  return (
    <div className="pagina">
      <h2>Novo lançamento</h2>
      <p className="pagina-sub">Registre uma entrada (crédito) ou saída (débito) de caixa.</p>

      {sucessoId && (
        <div className="alerta alerta-sucesso">
          Lançamento registrado com sucesso. <strong>ID:</strong> {sucessoId}
        </div>
      )}
      {erroGeral && <div className="alerta alerta-erro">{erroGeral}</div>}

      <form className="form-card" onSubmit={aoEnviar}>
        <Campo label="Tipo" htmlFor="tipo" erro={erro('Tipo')}>
          <select id="tipo" value={tipo} onChange={(e) => setTipo(e.target.value as TipoLancamento)}>
            <option value="Credito">Crédito</option>
            <option value="Debito">Débito</option>
          </select>
        </Campo>

        <Campo label="Valor" htmlFor="valor" erro={erro('Valor')}>
          <input
            id="valor"
            type="number"
            inputMode="decimal"
            step="0.01"
            min="0"
            placeholder="0,00"
            value={valor}
            onChange={(e) => setValor(e.target.value)}
            required
          />
        </Campo>

        <Campo label="Data" htmlFor="data" erro={erro('Data')}>
          <input
            id="data"
            type="date"
            value={data}
            onChange={(e) => setData(e.target.value)}
            required
          />
        </Campo>

        <Campo label="Descrição (opcional)" htmlFor="descricao" erro={erro('Descricao')}>
          <input
            id="descricao"
            type="text"
            maxLength={200}
            placeholder="Ex.: Venda balcão, pagamento fornecedor…"
            value={descricao}
            onChange={(e) => setDescricao(e.target.value)}
          />
        </Campo>

        <button type="submit" className="botao-primario" disabled={enviando}>
          {enviando ? 'Registrando…' : 'Registrar lançamento'}
        </button>
      </form>
    </div>
  )
}
