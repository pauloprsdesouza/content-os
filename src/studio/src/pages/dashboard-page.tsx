import { useEffect, useState } from "react"
import { Link } from "react-router"

import { Card, CardContent } from "@/components/ui/card"
import { ApiError, apiRequest } from "@/lib/api/client"
import { useAuth } from "@/lib/auth"

type DashboardSummary = {
  sourcesTotal: number
  claimsPendingReview: number
  researchJobsActive: number
  contentVersionsPendingApproval: number
  publicationPackagesReady: number
  purchasesConfirmed: number
  learnersActive: number
  outcomesCompleted: number
}

function greeting(userName: string | null | undefined) {
  const hour = new Date().getHours()
  const hello = hour < 12 ? "Bom dia" : hour < 18 ? "Boa tarde" : "Boa noite"
  const name = userName?.split("@")[0]
  return name ? `${hello}, ${name}` : hello
}

export function DashboardPage() {
  const { session } = useAuth()
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    apiRequest<DashboardSummary>("/api/v1/dashboard/summary")
      .then((payload) => {
        if (!cancelled) {
          setSummary(payload)
        }
      })
      .catch((requestError: unknown) => {
        if (!cancelled) {
          setError(
            requestError instanceof ApiError ? requestError.message : "Falha ao carregar o resumo.",
          )
        }
      })
    return () => {
      cancelled = true
    }
  }, [])

  const metrics = [
    { label: "Afirmações para revisar", value: summary?.claimsPendingReview ?? "…" },
    { label: "Fontes monitoradas", value: summary?.sourcesTotal ?? "…" },
    { label: "Pesquisas ativas", value: summary?.researchJobsActive ?? "…" },
    { label: "Conteúdos em aprovação", value: summary?.contentVersionsPendingApproval ?? "…" },
    { label: "Pacotes prontos", value: summary?.publicationPackagesReady ?? "…" },
    { label: "Compras confirmadas", value: summary?.purchasesConfirmed ?? "…" },
    { label: "Learners ativos", value: summary?.learnersActive ?? "…" },
    { label: "Outcomes concluídos", value: summary?.outcomesCompleted ?? "…" },
  ]

  const priorities = [
    summary && summary.claimsPendingReview > 0
      ? {
          href: "/claims",
          title: "Afirmações para revisar",
          detail: `${summary.claimsPendingReview} na fila`,
        }
      : null,
    summary && summary.contentVersionsPendingApproval > 0
      ? {
          href: "/conteudo",
          title: "Aprovar conteúdo",
          detail: `${summary.contentVersionsPendingApproval} aguardando`,
        }
      : null,
    summary && summary.publicationPackagesReady > 0
      ? {
          href: "/publicacao",
          title: "Exportar pacote",
          detail: `${summary.publicationPackagesReady} pronto(s)`,
        }
      : null,
  ].filter((item): item is { href: string; title: string; detail: string } => item !== null)

  return (
    <div className="space-y-6">
      <section>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
          {greeting(session?.userName)}
        </h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Aqui está o que precisa da sua atenção hoje. Os totais vêm da API.
        </p>
        {error && <p className="mt-2 text-sm text-[var(--destructive)]">{error}</p>}
      </section>

      {summary && (
        <Link
          className="block rounded-[18px] bg-[var(--primary)] px-6 py-5 text-white no-underline"
          to="/claims"
        >
          <p className="m-0 text-xs font-semibold text-[#d9dbff]">Próxima ação</p>
          <p className="mb-0 mt-2 text-lg font-semibold">Revisar evidências</p>
          <p className="mb-0 mt-2 text-xs">
            {summary.claimsPendingReview}{" "}
            {summary.claimsPendingReview === 1 ? "item pendente" : "itens pendentes"} →
          </p>
        </Link>
      )}

      <section className="grid gap-4 xl:grid-cols-[1fr_280px]">
        <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-2">
          {metrics.map((metric) => (
            <Card key={metric.label}>
              <CardContent className="p-5">
                <p className="m-0 text-xs font-medium uppercase tracking-wide text-[var(--muted-foreground)]">
                  {metric.label}
                </p>
                <p className="mb-0 mt-2 text-3xl font-bold tracking-tight">{metric.value}</p>
              </CardContent>
            </Card>
          ))}
        </div>
        <Card>
          <CardContent className="space-y-3 p-5">
            <p className="m-0 text-sm font-semibold">Trabalho prioritário</p>
            {!summary && <p className="m-0 text-sm text-[var(--muted-foreground)]">Carregando…</p>}
            {summary && priorities.length === 0 && (
              <p className="m-0 text-sm text-[var(--muted-foreground)]">Nada pendente agora.</p>
            )}
            {priorities.map((item, index) => (
              <Link key={item.href} className="block no-underline" to={item.href}>
                <p className="m-0 text-xs text-[var(--muted-foreground)]">{index + 1}</p>
                <p className="m-0 text-sm font-semibold text-[var(--foreground)]">{item.title}</p>
                <p className="m-0 text-xs text-[var(--muted-foreground)]">{item.detail}</p>
              </Link>
            ))}
          </CardContent>
        </Card>
      </section>
    </div>
  )
}
