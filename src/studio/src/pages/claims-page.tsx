import { useEffect, useState } from "react"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import {
  approveClaim,
  getClaim,
  listPendingClaims,
  rejectClaim,
  type ClaimDetail,
  type ClaimQueueItem,
} from "@/lib/api/knowledge"

export function ClaimsPage() {
  const [queue, setQueue] = useState<ClaimQueueItem[]>([])
  const [index, setIndex] = useState(0)
  const [claim, setClaim] = useState<ClaimDetail | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [notes, setNotes] = useState("")
  const [loading, setLoading] = useState(true)
  const [acting, setActing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [conflict, setConflict] = useState<string | null>(null)

  async function loadQueue(preferredIndex = 0) {
    setLoading(true)
    setError(null)
    setConflict(null)
    try {
      const page = await listPendingClaims()
      setQueue(page.items)
      if (page.items.length === 0) {
        setClaim(null)
        setEtag(null)
        setIndex(0)
        return
      }

      const nextIndex = Math.min(preferredIndex, page.items.length - 1)
      setIndex(nextIndex)
      await loadClaim(page.items[nextIndex].id)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível carregar as afirmações para revisar.",
      )
    } finally {
      setLoading(false)
    }
  }

  async function loadClaim(claimId: string) {
    const result = await getClaim(claimId)
    setClaim(result.data)
    setEtag(result.etag)
    setNotes("")
    setConflict(null)
  }

  useEffect(() => {
    void loadQueue()
  }, [])

  async function handleApprove() {
    if (!claim || !etag) {
      return
    }

    setActing(true)
    try {
      await approveClaim(claim.id, etag)
      toast.success("Afirmação aprovada")
      await loadQueue(index)
    } catch (requestError) {
      handleReviewError(requestError)
    } finally {
      setActing(false)
    }
  }

  async function handleReject() {
    if (!claim || !etag) {
      return
    }

    if (!notes.trim()) {
      toast.error("Informe o motivo da rejeição.")
      return
    }

    setActing(true)
    try {
      await rejectClaim(claim.id, etag, notes.trim())
      toast.success("Afirmação rejeitada")
      await loadQueue(index)
    } catch (requestError) {
      handleReviewError(requestError)
    } finally {
      setActing(false)
    }
  }

  function handleReviewError(requestError: unknown) {
    if (requestError instanceof ApiError && requestError.status === 412) {
      setConflict(
        "A claim mudou desde que você a abriu (412). Recarregue antes de decidir.",
      )
      toast.error("Conflito de versão (412)")
      return
    }

    if (requestError instanceof ApiError && requestError.status === 409) {
      setConflict(requestError.message || "Conflito ao revisar a claim (409).")
      toast.error("Conflito (409)")
      return
    }

    toast.error(
      requestError instanceof Error ? requestError.message : "Falha na revisão.",
    )
  }

  async function goTo(nextIndex: number) {
    if (nextIndex < 0 || nextIndex >= queue.length) {
      return
    }
    setIndex(nextIndex)
    setLoading(true)
    try {
      await loadClaim(queue[nextIndex].id)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível abrir a claim.",
      )
    } finally {
      setLoading(false)
    }
  }

  if (loading && !claim) {
    return <LoadingState label="Carregando fila de revisão…" />
  }

  if (error) {
    return <ErrorState title="Fila indisponível" description={error} />
  }

  if (!claim) {
    return (
      <div className="space-y-6">
        <div>
          <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
            Afirmações para revisar
          </h2>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Nenhuma afirmação pendente no momento.
          </p>
        </div>
        <EmptyState
          title="Fila vazia"
          description="Quando a pesquisa extrair afirmações, elas aparecerão aqui para revisão humana."
        />
      </div>
    )
  }

  const progress = queue.length === 0 ? 0 : ((index + 1) / queue.length) * 100

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
            Afirmações para revisar
          </h2>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Afirmação {index + 1} de {queue.length} · Atalhos: A aprova · R rejeita
          </p>
        </div>
        <Button variant="outline" type="button" onClick={() => void loadQueue(index)}>
          Recarregar
        </Button>
      </div>

      {conflict && (
        <div
          role="alert"
          className="rounded-[var(--radius-md)] border border-[var(--destructive)] bg-[color-mix(in_srgb,var(--destructive)_8%,white)] px-4 py-3 text-sm text-[var(--destructive)]"
        >
          {conflict}
        </div>
      )}

      <div className="grid gap-4 xl:grid-cols-[1.35fr_0.65fr]">
        <Card>
          <CardContent className="space-y-6 p-6">
            <div>
              <p className="m-0 text-[11px] font-bold uppercase tracking-[0.12em] text-[var(--primary)]">
                Afirmação proposta
              </p>
              <p className="mb-0 mt-3 text-2xl font-bold leading-snug tracking-[-0.02em] text-[var(--foreground)]">
                {claim.statement}
              </p>
              <p className="mb-0 mt-3 text-xs text-[var(--muted-foreground)]">
                Confiança {(claim.confidence * 100).toFixed(0)}% · versão {claim.version}
              </p>
            </div>

            <div>
              <p className="m-0 text-[11px] font-bold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                Evidências relacionadas
              </p>
              <div className="mt-4 space-y-3">
                {claim.evidence.length === 0 ? (
                  <div className="rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-4 py-8 text-center text-sm text-[var(--muted-foreground)]">
                    Sem evidências vinculadas.
                  </div>
                ) : (
                  claim.evidence.map((item) => (
                    <div
                      key={item.evidenceId}
                      className="rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-4 py-3 text-sm"
                    >
                      <p className="m-0 font-medium">{item.locator}</p>
                      <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                        {item.extractionMethod} · confiança {(item.confidence * 100).toFixed(0)}%
                      </p>
                    </div>
                  ))
                )}
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="space-y-5 p-6">
            <h3 className="m-0 text-lg font-semibold text-[var(--foreground)]">Decisão</h3>

            <div>
              <p className="mb-2 text-xs font-semibold text-[var(--muted-foreground)]">
                Notas do revisor / motivo da rejeição
              </p>
              <textarea
                className="min-h-24 w-full resize-none rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-[13px]"
                placeholder="Obrigatório para rejeitar."
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
              />
            </div>

            <div className="grid grid-cols-2 gap-3">
              <Button
                variant="destructive"
                disabled={acting}
                type="button"
                onClick={() => void handleReject()}
              >
                Rejeitar
              </Button>
              <Button disabled={acting} type="button" onClick={() => void handleApprove()}>
                Aprovar
              </Button>
            </div>

            <div className="flex items-center justify-between text-xs text-[var(--muted-foreground)]">
              <button
                type="button"
                className="hover:text-[var(--foreground)]"
                disabled={index <= 0}
                onClick={() => void goTo(index - 1)}
              >
                ← Afirmação anterior
              </button>
              <button
                type="button"
                className="hover:text-[var(--foreground)]"
                disabled={index >= queue.length - 1}
                onClick={() => void goTo(index + 1)}
              >
                Próxima afirmação →
              </button>
            </div>
            <div>
              <div className="mb-1 h-1.5 overflow-hidden rounded-full bg-[var(--background)]">
                <div
                  className="h-full rounded-full bg-[var(--primary)]"
                  style={{ width: `${progress}%` }}
                />
              </div>
              <p className="mb-0 text-center text-xs text-[var(--muted-foreground)]">
                {index + 1} de {queue.length}
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
