import { BrowserRouter, Route, Routes } from "react-router"
import { Toaster } from "sonner"

import { AppShell } from "@/components/app-shell"
import { ClaimsPage } from "@/pages/claims-page"
import { DashboardPage } from "@/pages/dashboard-page"
import { LoginPage } from "@/pages/login-page"
import { PlaceholderPage } from "@/pages/placeholder-page"
import { SourcesPage } from "@/pages/sources-page"

const placeholderRoutes = [
  { path: "pesquisa", name: "Pesquisa" },
  { path: "conteudo", name: "Conteúdo" },
  { path: "catalogo", name: "Catálogo" },
  { path: "publicacao", name: "Publicação" },
  { path: "comercio", name: "Comércio" },
  { path: "resultados", name: "Resultados" },
  { path: "admin", name: "Administração" },
]

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<AppShell />}>
          <Route index element={<DashboardPage />} />
          <Route path="fontes" element={<SourcesPage />} />
          <Route path="claims" element={<ClaimsPage />} />
          {placeholderRoutes.map(({ path, name }) => (
            <Route key={path} path={path} element={<PlaceholderPage name={name} />} />
          ))}
        </Route>
      </Routes>
      <Toaster richColors position="top-right" />
    </BrowserRouter>
  )
}
