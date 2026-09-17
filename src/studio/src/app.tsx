import { BrowserRouter, Navigate, Route, Routes } from "react-router"
import { Toaster } from "sonner"

import { AppShell } from "@/components/app-shell"
import { CatalogoPage } from "@/pages/catalogo-page"
import { ClaimsPage } from "@/pages/claims-page"
import { ConteudoPage } from "@/pages/conteudo-page"
import { DashboardPage } from "@/pages/dashboard-page"
import { LoginPage } from "@/pages/login-page"
import { PesquisaPage } from "@/pages/pesquisa-page"
import { PublicacaoPage } from "@/pages/publicacao-page"
import { ResultadosPage } from "@/pages/resultados-page"
import { SourceDetailPage } from "@/pages/source-detail-page"
import { SourcesPage } from "@/pages/sources-page"
import { VendasPage } from "@/pages/vendas-page"

export function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<AppShell />}>
          <Route index element={<DashboardPage />} />
          <Route path="fontes" element={<SourcesPage />} />
          <Route path="fontes/:sourceId" element={<SourceDetailPage />} />
          <Route path="claims" element={<ClaimsPage />} />
          <Route path="pesquisa" element={<PesquisaPage />} />
          <Route path="conteudo" element={<ConteudoPage />} />
          <Route path="produtos" element={<CatalogoPage />} />
          <Route path="publicacao" element={<PublicacaoPage />} />
          <Route path="vendas" element={<VendasPage />} />
          <Route path="resultados" element={<ResultadosPage />} />
          <Route path="catalogo" element={<Navigate to="/produtos" replace />} />
          <Route path="comercio" element={<Navigate to="/vendas" replace />} />
          <Route path="admin" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
      <Toaster richColors position="top-right" />
    </BrowserRouter>
  )
}
