import { BrowserRouter, Navigate, Route, Routes } from "react-router"
import { Toaster } from "sonner"

import { AppShell } from "@/components/app-shell"
import { RequireAuth } from "@/components/require-auth"
import { AuthProvider } from "@/lib/auth"
import { CatalogoPage } from "@/pages/catalogo-page"
import { ConteudoPage } from "@/pages/conteudo-page"
import { DashboardPage } from "@/pages/dashboard-page"
import { DesempenhoPage } from "@/pages/desempenho-page"
import { ProfilePage } from "@/pages/profile-page"
import { HelpPage } from "@/pages/help-page"
import { LoginPage } from "@/pages/login-page"
import { PesquisaPage } from "@/pages/pesquisa-page"
import { PreferencesPage } from "@/pages/preferences-page"
import { RevisaoPage } from "@/pages/revisao-page"
import { SourceDetailPage } from "@/pages/source-detail-page"
import { SourcesPage } from "@/pages/sources-page"

export function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<RequireAuth />}>
            <Route element={<AppShell />}>
              <Route index element={<DashboardPage />} />
              <Route path="edicoes" element={<CatalogoPage />} />
              <Route path="revisao" element={<RevisaoPage />} />
              <Route path="desempenho" element={<DesempenhoPage />} />
              <Route path="perfil" element={<ProfilePage />} />
              <Route path="perfil/seguranca" element={<ProfilePage />} />
              <Route path="preferencias" element={<PreferencesPage />} />
              <Route path="ajuda" element={<HelpPage />} />
              <Route path="fontes" element={<SourcesPage />} />
              <Route path="fontes/:sourceId" element={<SourceDetailPage />} />
              <Route path="pesquisa" element={<PesquisaPage />} />
              <Route path="conteudo" element={<ConteudoPage />} />
              <Route path="claims" element={<Navigate to="/revisao" replace />} />
              <Route path="produtos" element={<Navigate to="/edicoes" replace />} />
              <Route path="publicacao" element={<Navigate to="/edicoes" replace />} />
              <Route path="vendas" element={<Navigate to="/desempenho" replace />} />
              <Route path="resultados" element={<Navigate to="/desempenho?aba=resultados" replace />} />
              <Route path="catalogo" element={<Navigate to="/edicoes" replace />} />
              <Route path="comercio" element={<Navigate to="/desempenho" replace />} />
              <Route path="admin" element={<Navigate to="/" replace />} />
            </Route>
          </Route>
        </Routes>
        <Toaster richColors position="top-right" />
      </BrowserRouter>
    </AuthProvider>
  )
}
