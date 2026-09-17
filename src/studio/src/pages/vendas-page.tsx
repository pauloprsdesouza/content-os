import { useEffect, useState } from "react"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import {
  listPurchases,
  purchaseStatusLabel,
  runReconciliation,
  SIGNAL_PURCHASE_STATUSES,
  type PurchaseListItem,
} from "@/lib/api/commerce"
import { cn } from "@/lib/utils"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function VendasPage() {
  const [purchases, setPurchases] = useState<PurchaseListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [reconciling, setReconciling] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const page = await listPurchases()
      setPurchases(page.items)
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 401
          ? "Faça login para gerenciar vendas."
          : requestError instanceof Error
            ? requestError.message
            : "Não foi possível carregar as compras.",
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  async function handleReconcile() {
    setReconciling(true)
    try {
      const result = await runReconciliation()
      toast.success(
        `Reconciliação: ${result.confirmedCount} confirmada(s), ${result.ignoredCount} ignorada(s) de ${result.processedCount}.`,
      )
      await load()
    } catch (requestError) {
      toast.error(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível reconciliar.",
      )
    } finally {
      setReconciling(false)
    }
  }

  const signalCount = purchases.filter((p) =>
    SIGNAL_PURCHASE_STATUSES.has(p.status),
  ).length
  const confirmedCount = purchases.filter((p) => p.status === "Confirmed").length

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
            Comércio
          </p>
          <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Vendas</h1>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Webhook = sinal. Venda confirmada só após reconciliação com o provedor.
          </p>
        </div>
        <Button
          type="button"
          disabled={reconciling || loading}
          onClick={() => void handleReconcile()}
        >
          {reconciling ? "Reconciliando…" : "Executar reconciliação"}
        </Button>
      </header>

      {loading && <LoadingState label="Carregando compras…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}

      {!loading && !error && (
        <>
          <div className="grid gap-3 sm:grid-cols-2">
            <Card>
              <CardContent className="pt-6">
                <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                  Sinais (não confirmadas)
                </p>
                <p className="mb-0 mt-2 text-3xl font-semibold tabular-nums">{signalCount}</p>
              </CardContent>
            </Card>
            <Card>
              <CardContent className="pt-6">
                <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                  Confirmadas (pós-reconciliação)
                </p>
                <p className="mb-0 mt-2 text-3xl font-semibold tabular-nums">
                  {confirmedCount}
                </p>
              </CardContent>
            </Card>
          </div>

          {purchases.length === 0 ? (
            <EmptyState
              title="Nenhuma compra"
              description="Envie um webhook Kiwify de teste e depois execute a reconciliação."
            />
          ) : (
            <Card>
              <CardContent className="space-y-3 pt-6">
                <h2 className="m-0 text-base font-semibold">Compras</h2>
                <ul className="m-0 list-none space-y-3 p-0">
                  {purchases.map((purchase) => {
                    const isSignal = SIGNAL_PURCHASE_STATUSES.has(purchase.status)
                    return (
                      <li
                        key={purchase.id}
                        className="rounded-[var(--radius-md)] border border-[var(--border)] px-4 py-3"
                      >
                        <div className="flex flex-wrap items-start justify-between gap-2">
                          <div>
                            <p className="m-0 text-sm font-semibold">
                              {purchase.provider} · {purchase.externalId}
                            </p>
                            <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                              {purchase.buyerEmail ?? "sem e-mail"} ·{" "}
                              {formatWhen(purchase.createdAt)}
                            </p>
                          </div>
                          <span
                            className={cn(
                              "rounded-[var(--radius-sm)] px-2 py-1 text-xs font-medium",
                              isSignal
                                ? "bg-[var(--muted)] text-[var(--muted-foreground)]"
                                : purchase.status === "Confirmed"
                                  ? "bg-[var(--info)]/15 text-[var(--foreground)]"
                                  : "bg-[var(--muted)] text-[var(--muted-foreground)]",
                            )}
                          >
                            {purchaseStatusLabel(purchase.status)}
                          </span>
                        </div>
                        {isSignal && (
                          <p className="mb-0 mt-2 text-xs text-[var(--muted-foreground)]">
                            Ainda não é venda confirmada — execute a reconciliação.
                          </p>
                        )}
                        {purchase.status === "Confirmed" && purchase.confirmedAt && (
                          <p className="mb-0 mt-2 text-xs text-[var(--muted-foreground)]">
                            Confirmada em {formatWhen(purchase.confirmedAt)}
                            {purchase.learnerId
                              ? ` · learner ${purchase.learnerId.slice(0, 8)}…`
                              : ""}
                          </p>
                        )}
                      </li>
                    )
                  })}
                </ul>
              </CardContent>
            </Card>
          )}
        </>
      )}
    </div>
  )
}
