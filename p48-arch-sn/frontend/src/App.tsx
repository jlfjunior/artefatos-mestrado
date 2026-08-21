import { Navigate, Route, Routes } from 'react-router-dom'
import { PAPEIS_CONSOLIDADO, PAPEIS_LANCAMENTOS } from './auth/permissoes'
import { RotaProtegida } from './auth/RotaProtegida'
import { useAuth } from './auth/useAuth'
import { Layout } from './components/Layout'
import { Consolidado } from './pages/Consolidado'
import { Lancamentos } from './pages/Lancamentos'
import { Login } from './pages/Login'
import { Relatorio } from './pages/Relatorio'

export function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />

      <Route
        element={
          <RotaProtegida>
            <Layout />
          </RotaProtegida>
        }
      >
        <Route index element={<Inicio />} />
        <Route
          path="lancamentos"
          element={
            <RotaProtegida papeisPermitidos={PAPEIS_LANCAMENTOS}>
              <Lancamentos />
            </RotaProtegida>
          }
        />
        <Route
          path="consolidado"
          element={
            <RotaProtegida papeisPermitidos={PAPEIS_CONSOLIDADO}>
              <Consolidado />
            </RotaProtegida>
          }
        />
        <Route
          path="relatorio"
          element={
            <RotaProtegida papeisPermitidos={PAPEIS_CONSOLIDADO}>
              <Relatorio />
            </RotaProtegida>
          }
        />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

/**
 * Rota inicial: encaminha o usuário para a primeira página que seu papel
 * pode acessar, evitando deixá-lo numa tela vazia.
 */
function Inicio() {
  const { sessao } = useAuth()
  const papel = sessao?.papel

  if (papel === 'Operador') return <Navigate to="/lancamentos" replace />
  if (papel === 'Gerente') return <Navigate to="/consolidado" replace />
  // Admin acessa tudo; cai nos lançamentos por padrão.
  return <Navigate to="/lancamentos" replace />
}
