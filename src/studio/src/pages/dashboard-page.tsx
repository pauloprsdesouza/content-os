import { useEffect, useState } from "react"

import { Card, CardContent } from "@/components/ui/card"
import { ApiError, apiRequest } from "@/lib/api/client"

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

export function DashboardPage() {
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
    { label: "Claims aguardando revisão", value: summary?.claimsPendingReview ?? "…" },
    { label: "Fontes monitoradas", value: summary?.sourcesTotal ?? "…" },
    { label: "Pesquisas ativas", value: summary?.researchJobsActive ?? "…" },
    { label: "Conteúdos em aprovação", value: summary?.contentVersionsPendingApproval ?? "…" },
    { label: "Pacotes prontos", value: summary?.publicationPackagesReady ?? "…" },
    { label: "Compras confirmadas", value: summary?.purchasesConfirmed ?? "…" },
    { label: "Learners ativos", value: summary?.learnersActive ?? "…" },
    { label: "Outcomes concluídos", value: summary?.outcomesCompleted ?? "…" },
  ]

  return (
    <div className="space-y-6">
      <section>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
          Visão geral
        </h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Totais calculados pela API. Nada é inventado no Studio.
        </p>
        {error && <p className="mt-2 text-sm text-[var(--destructive)]">{error}</p>}
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
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
      </section>
    </div>
  )
}
