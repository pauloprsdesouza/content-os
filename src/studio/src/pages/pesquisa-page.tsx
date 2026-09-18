import { FormEvent, useEffect, useState } from "react"
import { Link, useSearchParams } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import { listSnapshots, listSources, type SnapshotItem, type SourceListItem } from "@/lib/api/knowledge"
import { watchOperation } from "@/lib/api/operations"
import {
  createResearchJob,
  deleteResearchJob,
  getResearchJob,
  getResearchJobFindings,
  listResearchJobs,
  type ResearchFinding,
  type ResearchJobDetail,
  type ResearchJobListItem,
} from "@/lib/api/research"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function PesquisaPage() {
  const [jobs, setJobs] = useState<ResearchJobListItem[]>([])
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [detail, setDetail] = useState<ResearchJobDetail | null>(null)
  const [findings, setFindings] = useState<ResearchFinding[]>([])
  const [loading, setLoading] = useState(true)
  const [creating, setCreating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [operationStatus, setOperationStatus] = useState<string | null>(null)
  const [sources, setSources] = useState<SourceListItem[]>([])
  const [snapshots, setSnapshots] = useState<SnapshotItem[]>([])
  const [searchParams] = useSearchParams()
  const [sourceId, setSourceId] = useState(searchParams.get("source") ?? "")
  const [snapshotId, setSnapshotId] = useState(searchParams.get("snapshot") ?? "")

  async function loadJobs(preferId?: string | null) {
    setLoading(true)
    setError(null)
    try {
      const page = await listResearchJobs()
      setJobs(page.items)
      const nextId = preferId === undefined ? (selectedId ?? page.items[0]?.id ?? null) : preferId
      setSelectedId(nextId)
      if (nextId) {
        await loadDetail(nextId)
      } else {
        setDetail(null)
        setFindings([])
      }
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 401
          ? "Faça login para monitorar pesquisas."
          : requestError instanceof Error
            ? requestError.message
            : "Não foi possível carregar as pesquisas.",
      )
    } finally {
      setLoading(false)
    }
  }

  async function loadDetail(jobId: string) {
    const [job, jobFindings] = await Promise.all([
      getResearchJob(jobId),
      getResearchJobFindings(jobId),
    ])
    setDetail(job)
    setFindings(jobFindings)
  }

  useEffect(() => {
    void loadJobs()
    void listSources()
      .then((page) => setSources(page.items))
      .catch(() => setSources([]))
  }, [])

  useEffect(() => {
    if (!sourceId) {
      setSnapshots([])
      return
    }
    void listSnapshots(sourceId)
      .then((page) => setSnapshots(page.items))
      .catch(() => setSnapshots([]))
  }, [sourceId])

  useEffect(() => {
    const operationId = detail?.operationId
    const jobId = detail?.id
    const status = detail?.status
    if (!operationId || !jobId || !status) {
      return
    }
    if (
      status === "AwaitingKnowledgeReview" ||
      status === "Completed" ||
      status === "Failed" ||
      status === "Cancelled"
    ) {
      return
    }

    const controller = new AbortController()
    void watchOperation(
      operationId,
      (operation) => setOperationStatus(operation.status),
      controller.signal,
    )
      .then(async (operation) => {
        setOperationStatus(operation.status)
        if (operation.status === "Succeeded") {
          toast.success("Pesquisa concluída (aguardando revisão de findings)")
        } else if (operation.status === "Failed") {
          toast.error(operation.errorMessage ?? "Falha na pesquisa")
        }

        const [job, jobFindings] = await Promise.all([
          getResearchJob(jobId),
          getResearchJobFindings(jobId),
        ])
        if (controller.signal.aborted) {
          return
        }
        setDetail(job)
        setFindings(jobFindings)
        setJobs((current) =>
          current.map((item) =>
            item.id === job.id
              ? { ...item, status: job.status, updatedAt: job.updatedAt }
              : item,
          ),
        )
      })
      .catch((requestError: unknown) => {
        if (requestError instanceof DOMException && requestError.name === "AbortError") {
          return
        }
      })

    return () => controller.abort()
  }, [detail?.id, detail?.operationId, detail?.status])

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setCreating(true)
    setOperationStatus(null)
    const formElement = event.currentTarget
    const form = new FormData(formElement)
    const topic = String(form.get("topic") ?? "").trim()
    const scopeNotes = String(form.get("scopeNotes") ?? "").trim()
    const sourceSnapshotId = String(form.get("sourceSnapshotId") ?? "").trim()
    if (!sourceSnapshotId) {
      toast.error("Escolha um snapshot antes de pesquisar.")
      setCreating(false)
      return
    }

    try {
      const accepted = await createResearchJob({
        topic,
        scopeNotes: scopeNotes || undefined,
        sourceSnapshotId,
      })
      toast.success("Pesquisa enfileirada")
      setOperationStatus("Accepted")
      formElement.reset()
      await loadJobs(accepted.subjectId)
    } catch (requestError) {
      toast.error(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível criar a pesquisa.",
      )
    } finally {
      setCreating(false)
    }
  }

  async function handleDeleteJob(job: ResearchJobListItem) {
    if (!window.confirm(`Apagar a pesquisa “${job.topic}”? Pesquisas com afirmações para revisar não podem ser apagadas.`)) {
      return
    }
    try {
      await deleteResearchJob(job.id)
      toast.success("Pesquisa apagada.")
      if (selectedId === job.id) {
        setSelectedId(null)
        setDetail(null)
        setFindings([])
      }
      await loadJobs(selectedId === job.id ? null : selectedId)
    } catch (requestError) {
      toast.error(requestError instanceof Error ? requestError.message : "Não foi possível apagar a pesquisa.")
    }
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
            Conhecimento
          </p>
          <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Pesquisa</h1>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Crie jobs de pesquisa e acompanhe o status da operação até findings.
            <Link className="ml-2 font-semibold text-[var(--primary)] hover:underline" to="/claims">
              Afirmações para revisar
            </Link>
          </p>
        </div>
      </header>

      <Card>
        <CardContent className="space-y-4 pt-6">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleCreate}>
            <div className="space-y-2">
              <Label htmlFor="topic">Tópico</Label>
              <Input id="topic" name="topic" required placeholder="Ex.: Provenance no Content OS" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="scopeNotes">Escopo (opcional)</Label>
              <Input id="scopeNotes" name="scopeNotes" placeholder="Notas de escopo" />
            </div>
            <div className="space-y-2">
              <Label htmlFor="sourceId">Fonte</Label>
              <select
                id="sourceId"
                className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
                value={sourceId}
                onChange={(event) => setSourceId(event.target.value)}
              >
                <option value="">Selecione</option>
                {sources.map((source) => (
                  <option key={source.id} value={source.id}>
                    {source.displayName}
                  </option>
                ))}
              </select>
            </div>
            <div className="space-y-2">
              <Label htmlFor="sourceSnapshotId">Snapshot</Label>
              <select
                id="sourceSnapshotId"
                name="sourceSnapshotId"
                required
                className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
                value={snapshotId}
                onChange={(event) => setSnapshotId(event.target.value)}
              >
                <option value="" disabled>
                  {snapshots.length === 0 ? "Nenhum snapshot nesta fonte" : "Selecione"}
                </option>
                {snapshots.map((snapshot) => (
                  <option key={snapshot.id} value={snapshot.id}>
                    {snapshot.capturedAt.slice(0, 16)} · {snapshot.contentHash.slice(0, 8)}
                  </option>
                ))}
              </select>
            </div>
            <div className="md:col-span-2">
              <Button type="submit" disabled={creating || snapshots.length === 0}>
                {creating ? "Enfileirando…" : "Nova pesquisa"}
              </Button>
            </div>
          </form>
          {operationStatus && (
            <p className="m-0 text-xs text-[var(--muted-foreground)]">
              Última operação: <strong>{operationStatus}</strong>
            </p>
          )}
        </CardContent>
      </Card>

      {loading && <LoadingState label="Carregando pesquisas…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}
      {!loading && !error && jobs.length === 0 && (
        <EmptyState
          title="Nenhuma pesquisa ainda"
          description="Crie um job para o worker stub gerar findings determinísticos."
        />
      )}

      {!loading && !error && jobs.length > 0 && (
        <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
          <Card>
            <CardContent className="space-y-2 pt-4">
              {jobs.map((job) => (
                <div
                  key={job.id}
                  className={`flex items-start gap-2 rounded-[var(--radius-md)] px-2 py-1 ${
                    selectedId === job.id ? "bg-[var(--accent)]" : ""
                  }`}
                >
                  <button
                    type="button"
                    className="min-w-0 flex-1 border-0 bg-transparent px-1 py-1 text-left text-sm"
                    onClick={() => {
                      setSelectedId(job.id)
                      void loadDetail(job.id)
                    }}
                  >
                    <div className="font-medium">{job.topic}</div>
                    <div className="text-xs text-[var(--muted-foreground)]">
                      {job.status} · {formatWhen(job.updatedAt)}
                    </div>
                  </button>
                  <Button type="button" size="sm" variant="ghost" onClick={() => void handleDeleteJob(job)}>
                    Excluir
                  </Button>
                </div>
              ))}
            </CardContent>
          </Card>

          <Card>
            <CardContent className="space-y-4 pt-6">
              {detail ? (
                <>
                  <div>
                    <h2 className="m-0 text-lg font-semibold">{detail.topic}</h2>
                    <p className="mb-0 mt-1 text-sm text-[var(--muted-foreground)]">
                      Status <strong>{detail.status}</strong>
                      {detail.operationId ? ` · operação ${detail.operationId.slice(0, 8)}…` : ""}
                    </p>
                    {detail.failureReason && (
                      <p className="mb-0 mt-2 text-sm text-[var(--destructive)]">
                        {detail.failureReason}
                      </p>
                    )}
                  </div>
                  <div>
                    <h3 className="m-0 text-sm font-semibold">Findings</h3>
                    {findings.length === 0 ? (
                      <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
                        {detail.status === "Failed" || detail.status === "Cancelled"
                          ? "Nenhum finding foi produzido."
                          : "Ainda sem findings — aguarde o worker."}
                      </p>
                    ) : (
                      <ul className="mt-2 space-y-2 pl-0">
                        {findings.map((finding) => (
                          <li
                            key={finding.id}
                            className="list-none rounded-[var(--radius-md)] border border-[var(--border)] px-3 py-2 text-sm"
                          >
                            {finding.statement}
                            <div className="mt-1 text-xs text-[var(--muted-foreground)]">
                              confiança {(finding.confidence * 100).toFixed(0)}%
                            </div>
                          </li>
                        ))}
                      </ul>
                    )}
                  </div>
                  <Button type="button" variant="outline" onClick={() => void loadDetail(detail.id)}>
                    Atualizar
                  </Button>
                </>
              ) : (
                <p className="m-0 text-sm text-[var(--muted-foreground)]">Selecione um job.</p>
              )}
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
