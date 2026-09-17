import {
  BarChart3,
  BookOpenCheck,
  Boxes,
  CircleUserRound,
  FileSearch,
  Gauge,
  Library,
  Megaphone,
  PenLine,
  Search,
  Settings2,
  ShoppingBag,
  Sparkles,
} from "lucide-react"
import { NavLink, Outlet, useLocation } from "react-router"

import { cn } from "@/lib/utils"

const navigation = [
  { label: "Dashboard", href: "/", icon: Gauge },
  { label: "Fontes", href: "/fontes", icon: Library },
  { label: "Pesquisa", href: "/pesquisa", icon: FileSearch },
  { label: "Claims", href: "/claims", icon: BookOpenCheck },
  { label: "Conteúdo", href: "/conteudo", icon: PenLine },
  { label: "Catálogo", href: "/catalogo", icon: Boxes },
  { label: "Publicação", href: "/publicacao", icon: Megaphone },
  { label: "Comércio", href: "/comercio", icon: ShoppingBag },
  { label: "Resultados", href: "/resultados", icon: BarChart3 },
  { label: "Admin", href: "/admin", icon: Settings2 },
]

const pageTitles: Record<string, string> = {
  "/": "Visão geral",
  "/fontes": "Fontes",
  "/pesquisa": "Pesquisa",
  "/claims": "Revisão de claims",
  "/conteudo": "Conteúdo",
  "/catalogo": "Catálogo",
  "/publicacao": "Publicação",
  "/comercio": "Comércio",
  "/resultados": "Resultados",
  "/admin": "Administração",
}

export function AppShell() {
  const location = useLocation()

  return (
    <div className="app-frame min-h-screen lg:grid lg:grid-cols-[268px_1fr]">
      <aside className="app-sidebar bg-[var(--surface-dark)] px-5 py-6 text-white lg:sticky lg:top-0 lg:h-screen">
        <div className="mb-8 flex items-center gap-3 px-2">
          <div className="grid size-10 place-items-center rounded-xl bg-[var(--accent)] text-[var(--accent-ink)]">
            <Sparkles className="size-5" />
          </div>
          <div>
            <div className="brand-wordmark text-[1.65rem] font-bold leading-none tracking-[-0.04em]">
              Content OS
            </div>
            <div className="mt-1 text-[0.62rem] font-semibold uppercase tracking-[0.2em] text-white/45">
              Studio de operações
            </div>
          </div>
        </div>

        <nav className="app-nav space-y-1" aria-label="Navegação principal">
          {navigation.map(({ label, href, icon: Icon }) => (
            <NavLink
              key={href}
              to={href}
              end={href === "/"}
              className={({ isActive }) =>
                cn(
                  "group flex items-center gap-3 rounded-[var(--radius-sm)] px-3 py-2.5 text-sm font-medium text-white/55 transition",
                  "hover:bg-white/[0.06] hover:text-white",
                  isActive && "bg-white/[0.09] text-white shadow-[inset_3px_0_0_var(--accent)]",
                )
              }
            >
              <Icon className="size-[18px] text-white/40 transition group-hover:text-[var(--accent)]" />
              {label}
            </NavLink>
          ))}
        </nav>

        <div className="mt-8 rounded-[var(--radius-md)] border border-white/10 bg-white/[0.04] p-4">
          <div className="mb-2 flex items-center gap-2 text-xs font-semibold uppercase tracking-wider text-[var(--accent)]">
            <span className="size-1.5 rounded-full bg-[var(--accent)] shadow-[0_0_10px_var(--accent)]" />
            Fase 0
          </div>
          <p className="m-0 text-xs leading-relaxed text-white/45">
            Fundação do produto e infraestrutura local.
          </p>
        </div>
      </aside>

      <div className="min-w-0">
        <header className="flex h-[76px] items-center justify-between border-b border-[var(--line)] bg-white/45 px-6 backdrop-blur-xl md:px-10">
          <div>
            <p className="m-0 text-[0.68rem] font-bold uppercase tracking-[0.18em] text-[var(--muted)]">
              Operações editoriais
            </p>
            <h1 className="m-0 mt-1 text-xl font-semibold tracking-[-0.025em]">
              {pageTitles[location.pathname] ?? "Content OS"}
            </h1>
          </div>
          <div className="flex items-center gap-3">
            <button
              className="hidden size-9 place-items-center rounded-full border border-[var(--line)] bg-white/60 text-[var(--muted)] md:grid"
              aria-label="Pesquisar"
            >
              <Search className="size-4" />
            </button>
            <div className="flex items-center gap-3 rounded-full border border-[var(--line)] bg-white/70 py-1.5 pl-2 pr-4">
              <CircleUserRound className="size-7 text-[var(--muted)]" />
              <div className="hidden text-left sm:block">
                <p className="m-0 text-xs font-semibold">Sessão local</p>
                <p className="m-0 text-[0.65rem] text-[var(--muted)]">Administrador</p>
              </div>
            </div>
          </div>
        </header>
        <main className="p-6 md:p-10">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
