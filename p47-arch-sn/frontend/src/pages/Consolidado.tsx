import { FormEvent, useState } from 'react'
import { consultarConsolidado } from '../api/services'
import { ApiError, Consolidado as ConsolidadoDto } from '../api/types'
import { formatarData, formatarMoeda, hojeIso } from '../utils/formato'

export function Consolidado() {
  const [data, setData] = useState(hojeIso())
  const [resultado, setResultado] = useState<ConsolidadoDto | null>(null)
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(false)

  async function aoConsultar(e: FormEvent) {
    e.preventDefault()
    setErro(null)
    setResultado(null)
    setCarregando(true)
    try {
      const consolidado = await consultarConsolidado(data)
      setResultado(consolidado)
    } catch (err) {
      if (err instanceof ApiError) {
        setErro(err.message)
      } else {
        setErro('Não foi possível consultar o consolidado.')
      }
    } finally {
      setCarregando(false)
    }
  }

  const semLancamentos =
    resultado !== null &&
    resultado.totalCreditos === 0 &&
    resultado.totalDebitos === 0 &&
    resultado.saldo === 0

  return (
    <div className="pagina">
      <h2>Consolidado diário</h2>
      <p className="pagina-sub">Selecione uma data para consultar o saldo do dia.</p>

      <form className="form-inline" onSubmit={aoConsultar}>
        <div className="campo">
          <label htmlFor="data-consulta">Data</label>
          <input
            id="data-consulta"
            type="date"
            value={data}
            onChange={(e) => setData(e.target.value)}
            required
          />
        </div>
        <button type="submit" className="botao-primario" disabled={carregando}>
          {carregando ? 'Consultando…' : 'Consultar'}
        </button>
      </form>

      {erro && <div className="alerta alerta-erro">{erro}</div>}

      {resultado && (
        <section className="resultado">
          <h3 className="resultado-titulo">Resumo de {formatarData(resultado.data)}</h3>

          {semLancamentos ? (
            <div className="alerta alerta-info">
              {resultado.mensagem ?? 'Não há lançamentos para esta data.'}
            </div>
          ) : (
            <div className="cards">
              <div className="card card-credito">
                <span className="card-rotulo">Total de créditos</span>
                <span className="card-valor">{formatarMoeda(resultado.totalCreditos)}</span>
              </div>
              <div className="card card-debito">
                <span className="card-rotulo">Total de débitos</span>
                <span className="card-valor">{formatarMoeda(resultado.totalDebitos)}</span>
              </div>
              <div className={resultado.saldo < 0 ? 'card card-saldo negativo' : 'card card-saldo'}>
                <span className="card-rotulo">Saldo</span>
                <span className="card-valor">{formatarMoeda(resultado.saldo)}</span>
              </div>
            </div>
          )}

          {resultado.atualizadoEmUtc && (
            <p className="resultado-rodape">
              Atualizado em {new Date(resultado.atualizadoEmUtc).toLocaleString('pt-BR')}
            </p>
          )}
        </section>
      )}
    </div>
  )
}
