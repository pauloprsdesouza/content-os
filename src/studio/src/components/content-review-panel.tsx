import { useEffect, useState } from "react"
import { Link } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import {
  approveContentVersion,
  getContentVersion,
  listContentUnits,
  requestContentChanges,
  type ContentUnitListItem,
  type ContentVersionDetail,
} from "@/lib/api/content"

export function ContentReviewPanel() {
  const [queue, setQueue] = useState<ContentUnitListItem[]>([])
  const [index, setIndex] = useState(0)
  const [version, setVersion] = useState<ContentVersionDetail | null>(null)
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
      const page = await listContentUnits(1, 100)
      const pending = page.items.filter(
        (unit) => unit.latestVersionId && unit.latestVersionStatus === "PendingHumanApproval",
      )
      setQueue(pending)
      if (pending.length === 0) {
        setVersion(null)
        setEtag(null)
        setIndex(0)
        return
      }
      const nextIndex = Math.min(preferredIndex, pending.length - 1)
      setIndex(nextIndex)
      await loadVersion(pending[nextIndex].latestVersionId as string)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível carregar as versões para revisar.",
      )
    } finally {
      setLoading(false)
    }
  }

  async function loadVersion(versionId: string) {
    const result = await getContentVersion(versionId)
    setVersion(result.data)
    setEtag(result.etag)
    setNotes("")
    setConflict(null)
  }

  useEffect(() => {
    void loadQueue()
  }, [])

  function handleError(requestError: unknown) {
    if (requestError instanceof ApiError && requestError.status === 412) {
      setConflict("A versão mudou (412). Recarregue antes de decidir.")
      toast.error("Conflito de versão (412)")
      return
    }
    toast.error(requestError instanceof Error ? requestError.message : "Falha na revisão.")
  }

  async function handleApprove() {
    if (!version || !etag) {
      return
    }
    setActing(true)
    try {
      await approveContentVersion(version.id, etag)
      toast.success("Versão aprovada. Ela ainda não entra no currículo sozinha.")
      await loadQueue(index)
    } catch (requestError) {
      handleError(requestError)
    } finally {
      setActing(false)
    }
  }

  async function handleRequestChanges() {
    if (!version || !etag) {
      return
    }
    if (!notes.trim()) {
      toast.error("Informe o que deve mudar.")
      return
    }
    setActing(true)
    try {
      await requestContentChanges(version.id, etag, notes.trim())
      toast.success("Alterações solicitadas")
      await loadQueue(index)
    } catch (requestError) {
      handleError(requestError)
    } finally {
      setActing(false)
    }
  }

  if (loading && !version) {
    return <LoadingState label="Carregando versões…" />
  }
  if (error) {
    return <ErrorState title="Fila indisponível" description={error} />
  }
  if (!version) {
    return (
      <div className="space-y-3">
        <EmptyState
          title="Nenhuma versão pronta"
          description="Quando o agente terminar a revisão, a versão aparece aqui. O rascunho continua em Novo conteúdo."
        />
        <Link className="text-sm font-semibold text-[var(--primary)]" to="/conteudo">
          Ir para novo conteúdo
        </Link>
      </div>
    )
  }

  const unit = queue[index]
  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h2 className="m-0 text-2xl font-bold tracking-[-0.03em]">
          Versão {index + 1} de {queue.length}
        </h2>
        <p className="m-0 text-sm text-[var(--muted-foreground)]">{unit?.title}</p>
      </div>
      {conflict && (
        <div role="alert" className="rounded-[var(--radius-md)] border border-[var(--destructive)] px-4 py-3 text-sm text-[var(--destructive)]">
          {conflict}
        </div>
      )}
      <div className="grid gap-4 xl:grid-cols-[1.4fr_0.6fr]">
        <Card>
          <CardContent className="space-y-5 p-6">
            <p className="m-0 inline-flex rounded-full bg-[color-mix(in_srgb,var(--primary)_12%,white)] px-2.5 py-1 text-[11px] font-semibold text-[var(--primary)]">
              Recomendação do agente · não é uma decisão
            </p>
            <pre className="m-0 max-h-80 overflow-auto whitespace-pre-wrap text-sm leading-relaxed text-[var(--foreground)]">
              {version.bodyMarkdown || "(corpo vazio)"}
            </pre>
            <p className="m-0 text-sm text-[var(--muted-foreground)]">
              Se você aprovar, a versão fica disponível para o currículo da edição. Ela não publica.
            </p>
            <div>
              <p className="mb-2 text-xs font-semibold">O que deve mudar</p>
              <textarea
                className="min-h-20 w-full resize-none rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-[13px]"
                placeholder="Obrigatório apenas se você pedir alterações."
                value={notes}
                onChange={(event) => setNotes(event.target.value)}
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button
                type="button"
                variant="outline"
                className="border-[var(--destructive)] text-[var(--destructive)]"
                disabled={acting}
                onClick={() => void handleRequestChanges()}
              >
                Pedir alterações
              </Button>
              <Button type="button" disabled={acting} onClick={() => void handleApprove()}>
                Aprovar versão
              </Button>
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="space-y-3 p-6">
            <h3 className="m-0 text-base font-semibold">Notas do agente</h3>
            <p className="m-0 text-sm text-[var(--muted-foreground)]">
              {version.agentReviewNotes || "O agente não deixou notas. A decisão continua sua."}
            </p>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
