import { useEffect, useState } from "react"

import { ErrorState, LoadingState } from "@/components/page-states"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import { getOutcomesSummary, type OutcomesSummary } from "@/lib/api/outcomes"

const cards: {
  key: keyof OutcomesSummary
  label: string
  hint: string
}[] = [
  {
    key: "confirmedPurchases",
    label: "Compras confirmadas",
    hint: "Após reconciliação",
  },
  {
    key: "learners",
    label: "Learners",
    hint: "Criados na confirmação",
  },
  {
    key: "enrollments",
    label: "Matrículas",
    hint: "Vinculadas à compra",
  },
  {
    key: "capstonesSubmitted",
    label: "Capstones enviados",
    hint: "Submissões",
  },
  {
    key: "evaluationsPassed",
    label: "Avaliações aprovadas",
    hint: "Passed = true",
  },
  {
    key: "outcomesRecorded",
    label: "Outcomes",
    hint: "Totais do backend",
  },
]

export function ResultadosPage({ embedded = false }: { embedded?: boolean }) {
  const [summary, setSummary] = useState<OutcomesSummary | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    async function load() {
      setLoading(true)
      setError(null)
      try {
        const data = await getOutcomesSummary()
        if (!cancelled) {
          setSummary(data)
        }
      } catch (requestError) {
        if (!cancelled) {
          setError(
            requestError instanceof ApiError && requestError.status === 401
              ? "Faça login para ver resultados."
              : requestError instanceof Error
                ? requestError.message
                : "Não foi possível carregar o resumo.",
          )
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }
    void load()
    return () => {
      cancelled = true
    }
  }, [])

  return (
    <div className="space-y-6">
      <header>
        {!embedded && (
          <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
            Aprendizagem
          </p>
        )}
        <h2 className="m-0 mt-1 text-xl font-semibold tracking-tight">Resultado do aluno</h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Totais calculados no backend — fórmulas não são duplicadas no Studio.
        </p>
      </header>

      {loading && <LoadingState label="Carregando resumo…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}

      {!loading && !error && summary && (
        <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
          {cards.map(({ key, label, hint }) => (
            <Card key={key}>
              <CardContent className="pt-6">
                <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                  {label}
                </p>
                <p className="mb-0 mt-2 text-3xl font-semibold tabular-nums">
                  {summary[key]}
                </p>
                <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">{hint}</p>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}
