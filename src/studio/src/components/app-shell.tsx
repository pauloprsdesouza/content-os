import {
  CircleHelp,
  Settings2,
} from "lucide-react"
import { NavLink, Outlet, useLocation } from "react-router"

import { cn } from "@/lib/utils"

const navigation = [
  { label: "Visão geral", href: "/", match: ["/"] },
  {
    label: "Conhecimento",
    href: "/fontes",
    match: ["/fontes", "/claims", "/pesquisa"],
  },
  { label: "Conteúdo", href: "/conteudo", match: ["/conteudo"] },
  { label: "Produtos", href: "/produtos", match: ["/produtos"] },
  { label: "Publicação", href: "/publicacao", match: ["/publicacao"] },
  { label: "Vendas", href: "/vendas", match: ["/vendas"] },
  { label: "Resultados", href: "/resultados", match: ["/resultados"] },
]

function isActivePath(pathname: string, match: string[]) {
  return match.some((path) =>
    path === "/" ? pathname === "/" : pathname === path || pathname.startsWith(`${path}/`),
  )
}

export function AppShell() {
  const location = useLocation()

  return (
    <div className="app-frame min-h-screen lg:grid lg:grid-cols-[var(--sidebar-width)_1fr]">
      <aside className="app-sidebar flex flex-col bg-[var(--sidebar)] px-4 py-7 text-white lg:sticky lg:top-0 lg:h-screen">
        <div className="mb-10 px-3">
          <div className="brand-mark text-[15px] font-bold leading-none tracking-[0.02em]">
            CONTENT
          </div>
          <div className="mt-1.5 text-[10px] font-bold uppercase tracking-[0.14em] text-[var(--info)]">
            STUDIO
          </div>
        </div>

        <nav className="app-nav flex-1 space-y-1" aria-label="Navegação principal">
          {navigation.map(({ label, href, match }) => {
            const active = isActivePath(location.pathname, match)
            return (
              <NavLink
                key={href}
                to={href}
                end={href === "/"}
                className={cn(
                  "relative flex items-center gap-3 rounded-[var(--radius-md)] px-3 py-2.5 text-[13px] font-medium transition",
                  active
                    ? "bg-[var(--sidebar-active)] text-white"
                    : "text-[var(--sidebar-foreground)] hover:bg-white/[0.04] hover:text-white",
                )}
              >
                <span
                  className={cn(
                    "size-2 shrink-0 rounded-full",
                    active ? "bg-[var(--info)]" : "bg-[var(--sidebar-muted)]/50",
                  )}
                  aria-hidden
                />
                {label}
                {active && (
                  <span className="absolute inset-y-2 left-0 w-0.5 rounded-full bg-[var(--info)]" />
                )}
              </NavLink>
            )
          })}
        </nav>

        <div className="mt-6 flex items-center gap-3 px-2 pt-4">
          <div className="grid size-8 place-items-center rounded-full bg-[var(--sidebar-active)] text-[11px] font-semibold">
            PR
          </div>
          <div className="min-w-0">
            <p className="m-0 truncate text-xs font-semibold">Paulo Roberto</p>
            <p className="m-0 text-[10px] text-[var(--sidebar-muted)]">Admin</p>
          </div>
        </div>
      </aside>

      <div className="min-w-0">
        <header className="flex h-16 items-center justify-between gap-4 border-b border-[var(--border)] bg-[var(--topbar)] px-6 md:px-8">
          <label className="flex min-w-0 flex-1 items-center gap-2 rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-[13px] text-[var(--muted-foreground)] md:max-w-md">
            <span aria-hidden>⌕</span>
            <input
              className="w-full border-0 bg-transparent text-[13px] text-[var(--foreground)] outline-none placeholder:text-[var(--muted-foreground)]"
              placeholder="Buscar em todo o workspace"
              type="search"
              aria-label="Buscar em todo o workspace"
            />
          </label>
          <div className="flex items-center gap-1 text-[var(--muted-foreground)]">
            <button
              type="button"
              className="grid size-9 place-items-center rounded-[var(--radius-sm)] hover:bg-[var(--background)]"
              aria-label="Ajuda"
            >
              <CircleHelp className="size-4" />
            </button>
            <button
              type="button"
              className="grid size-9 place-items-center rounded-[var(--radius-sm)] hover:bg-[var(--background)]"
              aria-label="Configurações"
            >
              <Settings2 className="size-4" />
            </button>
            <div
              className="ml-1 grid size-8 place-items-center rounded-full bg-[var(--accent)] text-[11px] font-semibold text-[var(--primary)]"
              aria-hidden
            >
              ◉
            </div>
          </div>
        </header>
        <main className="p-6 md:px-8 md:py-7">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
