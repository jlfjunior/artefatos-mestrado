import { FormEvent, useState } from 'react'
import { consultarRelatorio } from '../api/services'
import { ApiError, RelatorioConsolidado } from '../api/types'
import { formatarData, formatarMoeda, hojeIso } from '../utils/formato'

function inicioDoMesIso(): string {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-01`
}

export function Relatorio() {
  const [de, setDe] = useState(inicioDoMesIso())
  const [ate, setAte] = useState(hojeIso())
  const [resultado, setResultado] = useState<RelatorioConsolidado | null>(null)
  const [erro, setErro] = useState<string | null>(null)
  const [carregando, setCarregando] = useState(false)

  async function aoGerar(e: FormEvent) {
    e.preventDefault()
    setErro(null)
    setResultado(null)
    setCarregando(true)
    try {
      setResultado(await consultarRelatorio(de, ate))
    } catch (err) {
      setErro(err instanceof ApiError ? err.message : 'Não foi possível gerar o relatório.')
    } finally {
      setCarregando(false)
    }
  }

  return (
    <div className="pagina">
      <h2>Relatório do saldo diário consolidado</h2>
      <p className="pagina-sub">Escolha um período para ver o saldo consolidado dia a dia.</p>

      <form className="form-inline" onSubmit={aoGerar}>
        <div className="campo">
          <label htmlFor="de">De</label>
          <input id="de" type="date" value={de} max={ate} onChange={(e) => setDe(e.target.value)} required />
        </div>
        <div className="campo">
          <label htmlFor="ate">Até</label>
          <input id="ate" type="date" value={ate} min={de} onChange={(e) => setAte(e.target.value)} required />
        </div>
        <button type="submit" className="botao-primario" disabled={carregando}>
          {carregando ? 'Gerando…' : 'Gerar relatório'}
        </button>
      </form>

      {erro && <div className="alerta alerta-erro">{erro}</div>}

      {resultado && (
        <section className="resultado">
          <h3 className="resultado-titulo">
            Período de {formatarData(resultado.de)} a {formatarData(resultado.ate)}
          </h3>

          {resultado.dias.length === 0 ? (
            <div className="alerta alerta-info">Não há lançamentos no período selecionado.</div>
          ) : (
            <>
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
                  <span className="card-rotulo">Saldo do período</span>
                  <span className="card-valor">{formatarMoeda(resultado.saldo)}</span>
                </div>
              </div>

              <table className="tabela">
                <thead>
                  <tr>
                    <th>Data</th>
                    <th className="num">Créditos</th>
                    <th className="num">Débitos</th>
                    <th className="num">Saldo</th>
                  </tr>
                </thead>
                <tbody>
                  {resultado.dias.map((dia) => (
                    <tr key={dia.data}>
                      <td>{formatarData(dia.data)}</td>
                      <td className="num">{formatarMoeda(dia.totalCreditos)}</td>
                      <td className="num">{formatarMoeda(dia.totalDebitos)}</td>
                      <td className={dia.saldo < 0 ? 'num negativo' : 'num'}>{formatarMoeda(dia.saldo)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}
        </section>
      )}
    </div>
  )
}
