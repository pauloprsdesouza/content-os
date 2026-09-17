import { FormEvent, useEffect, useState } from "react"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import {
  approveContentVersion,
  createContentUnit,
  getContentVersion,
  listContentUnits,
  requestContentChanges,
  requestContentReview,
  type ContentUnitListItem,
  type ContentVersionDetail,
} from "@/lib/api/content"
import { pollOperationUntilSettled } from "@/lib/api/operations"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function ConteudoPage() {
  const [units, setUnits] = useState<ContentUnitListItem[]>([])
  const [selectedVersionId, setSelectedVersionId] = useState<string | null>(null)
  const [version, setVersion] = useState<ContentVersionDetail | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [notes, setNotes] = useState("")
  const [loading, setLoading] = useState(true)
  const [acting, setActing] = useState(false)
  const [creating, setCreating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [conflict, setConflict] = useState<string | null>(null)

  async function loadUnits(preferVersionId?: string | null) {
    setLoading(true)
    setError(null)
    try {
      const page = await listContentUnits()
      setUnits(page.items)
      const nextVersionId =
        preferVersionId ??
        selectedVersionId ??
        page.items.find((unit) => unit.latestVersionId)?.latestVersionId ??
        null
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
            : "Não foi possível carregar as unidades.",
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

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setCreating(true)
    const form = new FormData(event.currentTarget)
    const title = String(form.get("title") ?? "").trim()
    const brief = String(form.get("brief") ?? "").trim()

    try {
      const response = await createContentUnit({
        title,
        brief: brief || undefined,
        queueGeneration: true,
      })

      if ("operationId" in response) {
        toast.success("Geração enfileirada")
        const operation = await pollOperationUntilSettled(response.operationId)
        if (operation.status === "Succeeded") {
          toast.success("Rascunho gerado")
        } else if (operation.status === "Failed") {
          toast.error(operation.errorMessage ?? "Falha na geração")
        }
        await loadUnits()
      } else {
        toast.success("Unidade criada")
        await loadUnits(response.versionId)
      }
      event.currentTarget.reset()
    } catch (requestError) {
      toast.error(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível criar a unidade.",
      )
    } finally {
      setCreating(false)
    }
  }

  async function handleRequestReview() {
    if (!version) {
      return
    }
    setActing(true)
    try {
      const accepted = await requestContentReview(version.id)
      toast.success("Revisão solicitada")
      await pollOperationUntilSettled(accepted.operationId)
      await loadVersion(version.id)
      await loadUnits(version.id)
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

  const canApprove = version?.status === "PendingHumanApproval"

  return (
    <div className="space-y-6">
      <header>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
          Autoria
        </p>
        <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Conteúdo</h1>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Unidades, versões e gate humano de aprovação.
        </p>
      </header>

      <Card>
        <CardContent className="space-y-4 pt-6">
          <form className="grid gap-4 md:grid-cols-[1fr_1fr_auto]" onSubmit={handleCreate}>
            <div className="space-y-2">
              <Label htmlFor="title">Título</Label>
              <Input id="title" name="title" required placeholder="Ex.: Guia de provenance" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="brief">Brief (opcional)</Label>
              <Input id="brief" name="brief" placeholder="Direção editorial" />
            </div>
            <div className="flex items-end">
              <Button type="submit" disabled={creating}>
                {creating ? "Gerando…" : "Nova unidade"}
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      {loading && <LoadingState label="Carregando conteúdo…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}
      {!loading && !error && units.length === 0 && (
        <EmptyState
          title="Nenhuma unidade"
          description="Crie uma unidade para o worker stub gerar um ContentVersion em markdown."
        />
      )}

      {!loading && !error && units.length > 0 && (
        <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
          <Card>
            <CardContent className="space-y-2 pt-4">
              {units.map((unit) => (
                <button
                  key={unit.id}
                  type="button"
                  disabled={!unit.latestVersionId}
                  className={`block w-full rounded-[var(--radius-md)] px-3 py-2 text-left text-sm transition ${
                    selectedVersionId === unit.latestVersionId
                      ? "bg-[var(--accent)]"
                      : "hover:bg-[var(--muted)]"
                  }`}
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
                    {unit.latestVersionStatus ?? "sem versão"} · {formatWhen(unit.updatedAt)}
                  </div>
                </button>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="space-y-4 pt-6">
              {version ? (
                <>
                  <div>
                    <h2 className="m-0 text-lg font-semibold">
                      Revisão {version.revision} · {version.status}
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
                  Selecione uma unidade.
                </p>
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
