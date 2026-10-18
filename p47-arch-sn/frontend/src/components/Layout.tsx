import { NavLink, Outlet } from 'react-router-dom'
import { PAPEIS_CONSOLIDADO, PAPEIS_LANCAMENTOS, podeAcessar } from '../auth/permissoes'
import { useAuth } from '../auth/useAuth'

export function Layout() {
  const { sessao, sair } = useAuth()
  const papel = sessao?.papel

  return (
    <div className="app-shell">
      <header className="topo">
        <div className="topo-marca">
          <span className="marca-titulo">Fluxo de Caixa</span>
        </div>

        <nav className="menu">
          {podeAcessar(papel, PAPEIS_LANCAMENTOS) && (
            <NavLink to="/lancamentos" className={navClass}>
              Lançamentos
            </NavLink>
          )}
          {podeAcessar(papel, PAPEIS_CONSOLIDADO) && (
            <NavLink to="/consolidado" className={navClass}>
              Consolidado
            </NavLink>
          )}
          {podeAcessar(papel, PAPEIS_CONSOLIDADO) && (
            <NavLink to="/relatorio" className={navClass}>
              Relatório
            </NavLink>
          )}
        </nav>

        <div className="topo-usuario">
          <span className="usuario-info">
            {sessao?.usuario} <span className="papel-badge">{papel}</span>
          </span>
          <button type="button" className="botao-sair" onClick={sair}>
            Sair
          </button>
        </div>
      </header>

      <main className="conteudo">
        <Outlet />
      </main>
    </div>
  )
}

function navClass({ isActive }: { isActive: boolean }) {
  return isActive ? 'menu-item ativo' : 'menu-item'
}
