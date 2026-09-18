import { useEffect, useState } from "react"
import { toast } from "sonner"

import { NovoConteudoWizard } from "@/components/novo-conteudo-wizard"
import { SeriesPanel } from "@/components/series-panel"
import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import {
  approveContentVersion,
  deleteContentUnit,
  getContentVersion,
  listContentUnits,
  requestContentChanges,
  requestContentReview,
  type ContentUnitListItem,
  type ContentVersionDetail,
} from "@/lib/api/content"
import { listProducts } from "@/lib/api/catalog"
import { watchOperation } from "@/lib/api/operations"
import { taskStatus } from "@/lib/task-status"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function ConteudoPage() {
  const [units, setUnits] = useState<ContentUnitListItem[]>([])
  const [productNames, setProductNames] = useState<Record<string, string>>({})
  const [selectedVersionId, setSelectedVersionId] = useState<string | null>(null)
  const [version, setVersion] = useState<ContentVersionDetail | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [notes, setNotes] = useState("")
  const [loading, setLoading] = useState(true)
  const [acting, setActing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [conflict, setConflict] = useState<string | null>(null)

  async function loadUnits(preferVersionId?: string | null) {
    setLoading(true)
    setError(null)
    try {
      const [page, products] = await Promise.all([listContentUnits(), listProducts()])
      setUnits(page.items)
      setProductNames(Object.fromEntries(products.items.map((product) => [product.id, product.name])))
      const nextVersionId =
        preferVersionId === undefined
          ? (selectedVersionId ?? page.items.find((unit) => unit.latestVersionId)?.latestVersionId ?? null)
          : preferVersionId
      setSelectedVersionId(nextVersionId)
      if (nextVersionId) {
        await loadVersion(nextVersionId)
      } else {
        setVersion(null)
        setEtag(null)
      }
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 401
          ? "Faça login para gerenciar conteúdo."
          : requestError instanceof Error
            ? requestError.message
            : "Não foi possível carregar o conteúdo.",
      )
    } finally {
      setLoading(false)
    }
  }

  async function loadVersion(versionId: string) {
    const result = await getContentVersion(versionId)
    setVersion(result.data)
    setEtag(result.etag)
    setConflict(null)
    setNotes("")
  }

  useEffect(() => {
    void loadUnits()
  }, [])

  useEffect(() => {
    const operationId = version?.operationId
    const versionId = version?.id
    const status = version?.status
    if (!operationId || !versionId || (status !== "GenerationQueued" && status !== "ReviewQueued")) {
      return
    }

    const controller = new AbortController()
    void watchOperation(operationId, () => undefined, controller.signal)
      .then(async (operation) => {
        if (controller.signal.aborted) {
          return
        }
        if (operation.status === "Succeeded") {
          toast.success(status === "ReviewQueued" ? "Revisão do agente concluída" : "Rascunho gerado")
        } else if (operation.status === "Failed") {
          toast.error(operation.errorMessage ?? "Falha na operação de conteúdo")
        }
        const refreshed = await getContentVersion(versionId)
        if (controller.signal.aborted) {
          return
        }
        setVersion(refreshed.data)
        setEtag(refreshed.etag)
        setUnits((current) =>
          current.map((unit) =>
            unit.latestVersionId === versionId
              ? {
                  ...unit,
                  latestVersionStatus: refreshed.data.status,
                  updatedAt: refreshed.data.updatedAt,
                }
              : unit,
          ),
        )
      })
      .catch((requestError: unknown) => {
        if (requestError instanceof DOMException && requestError.name === "AbortError") {
          return
        }
      })

    return () => controller.abort()
  }, [version?.id, version?.operationId, version?.status])

  async function handleRequestReview() {
    if (!version) {
      return
    }
    setActing(true)
    try {
      await requestContentReview(version.id)
      toast.success("Revisão solicitada")
      await loadVersion(version.id)
    } catch (requestError) {
      handleActionError(requestError)
    } finally {
      setActing(false)
    }
  }

  async function handleApprove() {
    if (!version || !etag) {
      return
    }
    setActing(true)
    try {
      await approveContentVersion(version.id, etag)
      toast.success("Versão aprovada")
      await loadUnits(version.id)
    } catch (requestError) {
      handleActionError(requestError)
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
      await loadUnits(version.id)
    } catch (requestError) {
      handleActionError(requestError)
    } finally {
      setActing(false)
    }
  }

  function handleActionError(requestError: unknown) {
    if (requestError instanceof ApiError && requestError.status === 412) {
      setConflict("A versão mudou (412). Recarregue antes de decidir.")
      toast.error("Conflito de versão (412)")
      return
    }
    toast.error(
      requestError instanceof Error ? requestError.message : "Falha na ação de conteúdo.",
    )
  }

  async function handleDeleteUnit(unit: ContentUnitListItem) {
    if (!window.confirm(`Apagar “${unit.title}”? Se já estiver no currículo, a exclusão é recusada.`)) {
      return
    }
    try {
      await deleteContentUnit(unit.id)
      toast.success("Conteúdo apagado.")
      if (selectedVersionId === unit.latestVersionId) {
        setSelectedVersionId(null)
        setVersion(null)
      }
      await loadUnits(selectedVersionId === unit.latestVersionId ? null : selectedVersionId)
    } catch (requestError) {
      toast.error(requestError instanceof Error ? requestError.message : "Não foi possível apagar o conteúdo.")
    }
  }

  const canApprove = version?.status === "PendingHumanApproval"

  return (
    <div className="space-y-6">
      <header>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
          Autoria
        </p>
        <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Novo conteúdo</h1>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Produto, formato e origem. O rascunho escreve sozinho. Revise aqui. O currículo do produto só recebe o que você aprovar.
        </p>
      </header>

      <Card>
        <CardContent className="space-y-6 pt-6">
          <NovoConteudoWizard onCreated={async () => loadUnits()} />
          <SeriesPanel onCreated={async () => loadUnits()} />
        </CardContent>
      </Card>

      {loading && <LoadingState label="Carregando conteúdo…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}
      {!loading && !error && units.length === 0 && (
        <EmptyState
          title="Nenhum conteúdo"
          description="Comece pelo produto, depois o formato e a origem."
        />
      )}

      {!loading && !error && units.length > 0 && (
        <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
          <Card>
            <CardContent className="space-y-2 pt-4">
              {units.map((unit) => (
                <div
                  key={unit.id}
                  className={`flex items-start gap-2 rounded-[var(--radius-md)] px-2 py-1 ${
                    selectedVersionId === unit.latestVersionId ? "bg-[var(--accent)]" : ""
                  }`}
                >
                  <button
                    type="button"
                    disabled={!unit.latestVersionId}
                    className="min-w-0 flex-1 border-0 bg-transparent px-1 py-1 text-left text-sm"
                    onClick={() => {
                      if (!unit.latestVersionId) {
                        return
                      }
                      setSelectedVersionId(unit.latestVersionId)
                      void loadVersion(unit.latestVersionId)
                    }}
                  >
                    <div className="font-medium">{unit.title}</div>
                    <div className="text-xs text-[var(--muted-foreground)]">
                      {unit.productId ? (productNames[unit.productId] ?? "Produto") : "Sem produto"} ·{" "}
                      {taskStatus(unit.latestVersionStatus)} · {formatWhen(unit.updatedAt)}
                    </div>
                  </button>
                  <Button type="button" size="sm" variant="ghost" onClick={() => void handleDeleteUnit(unit)}>
                    Excluir
                  </Button>
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="space-y-4 pt-6">
              {version ? (
                <>
                  <div>
                    <h2 className="m-0 text-lg font-semibold">
                      Revisão {version.revision} · {taskStatus(version.status)}
                    </h2>
                    <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                      ETag {etag ?? "—"} · atualizado {formatWhen(version.updatedAt)}
                    </p>
                  </div>
                  {conflict && (
                    <p className="m-0 rounded-[var(--radius-md)] bg-[var(--destructive)]/10 px-3 py-2 text-sm text-[var(--destructive)]">
                      {conflict}
                    </p>
                  )}
                  <pre className="max-h-80 overflow-auto whitespace-pre-wrap rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--muted)]/40 p-4 text-sm">
                    {version.bodyMarkdown || "(corpo vazio — aguarde geração)"}
                  </pre>
                  {version.agentReviewNotes && (
                    <p className="m-0 text-sm text-[var(--muted-foreground)]">
                      Agente: {version.agentReviewNotes}
                    </p>
                  )}
                  <div className="flex flex-wrap gap-2">
                    <Button
                      type="button"
                      variant="outline"
                      disabled={acting}
                      onClick={() => void handleRequestReview()}
                    >
                      Pedir revisão
                    </Button>
                    <Button
                      type="button"
                      disabled={acting || !canApprove}
                      onClick={() => void handleApprove()}
                    >
                      Aprovar
                    </Button>
                  </div>
                  {canApprove && (
                    <div className="space-y-2">
                      <Label htmlFor="changes">Solicitar alterações</Label>
                      <Input
                        id="changes"
                        value={notes}
                        onChange={(event) => setNotes(event.target.value)}
                        placeholder="O que precisa mudar?"
                      />
                      <Button
                        type="button"
                        variant="outline"
                        disabled={acting}
                        onClick={() => void handleRequestChanges()}
                      >
                        Enviar pedido de mudanças
                      </Button>
                    </div>
                  )}
                  <Button
                    type="button"
                    variant="ghost"
                    onClick={() => void loadVersion(version.id)}
                  >
                    Recarregar versão
                  </Button>
                </>
              ) : (
                <p className="m-0 text-sm text-[var(--muted-foreground)]">
                  Selecione um conteúdo.
                </p>
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
