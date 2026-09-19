import { FormEvent, useEffect, useState } from "react"
import { Link, useNavigate } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import { captureSource, deleteSource, listSources, uploadSourceFile, type SourceListItem } from "@/lib/api/knowledge"

function formatUpdatedAt(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function SourcesPage() {
  const [items, setItems] = useState<SourceListItem[]>([])
  const [totalItems, setTotalItems] = useState(0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [showCreate, setShowCreate] = useState(false)
  const [creating, setCreating] = useState(false)
  const [mode, setMode] = useState<"web" | "file" | "text">("web")
  async function handleDelete(source: SourceListItem) {
    if (!window.confirm(`Apagar “${source.displayName}”? Fontes já usadas como evidência não podem ser apagadas.`)) {
      return
    }
    try {
      await deleteSource(source.id)
      toast.success("Fonte apagada.")
      await loadSources()
    } catch (requestError) {
      toast.error(requestError instanceof Error ? requestError.message : "Não foi possível apagar a fonte.")
    }
  }

  const navigate = useNavigate()

  async function loadSources() {
    setLoading(true)
    setError(null)
    try {
      const page = await listSources()
      setItems(page.items)
      setTotalItems(page.totalItems)
    } catch (requestError) {
      if (requestError instanceof ApiError && requestError.status === 401) {
        setError("Faça login para ver as fontes.")
      } else {
        setError(
          requestError instanceof Error
            ? requestError.message
            : "Não foi possível carregar as fontes.",
        )
      }
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadSources()
  }, [])

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setCreating(true)
    const formElement = event.currentTarget
    const form = new FormData(formElement)
    const displayName = String(form.get("displayName") ?? "").trim()

    try {
      const captured =
        mode === "file"
          ? await uploadSourceFile(displayName, form.get("file") as File)
          : await captureSource({
              mode,
              displayName,
              location: String(form.get("location") ?? "").trim() || undefined,
              text: mode === "text" ? String(form.get("text") ?? "") : undefined,
            })
      toast.success("Snapshot capturado")
      setShowCreate(false)
      formElement.reset()
      await loadSources()
      navigate(`/pesquisa?source=${captured.sourceId}&snapshot=${captured.snapshotId}`)
    } catch (requestError) {
      const message =
        requestError instanceof ApiError && requestError.status === 409
          ? "Já existe uma fonte com essa URI."
          : requestError instanceof Error
            ? requestError.message
            : "Falha ao cadastrar fonte."
      toast.error(message)
    } finally {
      setCreating(false)
    }
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
            Fontes
          </h2>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Cadastro, snapshot e pesquisa. A decisão sobre o que entra fica em Revisão.
          </p>
          <p className="mb-0 mt-3 text-xs text-[var(--muted-foreground)]">
            <Link className="font-semibold text-[var(--primary)] hover:underline" to="/pesquisa">
              Pesquisa
            </Link>
            {" · "}
            <Link className="font-semibold text-[var(--primary)] hover:underline" to="/revisao">
              Revisão
            </Link>
          </p>
        </div>
        <Button type="button" onClick={() => setShowCreate((value) => !value)}>
          Adicionar fonte
        </Button>
      </div>

      {showCreate && (
        <Card>
          <CardContent className="p-6">
            <form className="grid gap-4 md:grid-cols-2" onSubmit={handleCreate}>
              <div className="flex gap-2 md:col-span-2">
                {(
                  [
                    ["web", "URL"],
                    ["file", "Arquivo"],
                    ["text", "Texto"],
                  ] as const
                ).map(([value, label]) => (
                  <button
                    key={value}
                    type="button"
                    className={`rounded-full px-3 py-1 text-xs font-semibold ${
                      mode === value
                        ? "bg-[var(--primary)] text-white"
                        : "bg-[var(--background)] text-[var(--muted-foreground)]"
                    }`}
                    onClick={() => setMode(value)}
                  >
                    {label}
                  </button>
                ))}
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="displayName">Nome</Label>
                <Input id="displayName" name="displayName" required placeholder="Documentação da fonte" />
              </div>
              {mode !== "file" && (
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="location">URL</Label>
                  <Input
                    id="location"
                    name="location"
                    required={mode === "web"}
                    placeholder="https://exemplo.com/documento"
                  />
                </div>
              )}
              {mode === "file" && (
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="file">Arquivo de texto, markdown ou PDF</Label>
                  <Input id="file" name="file" type="file" accept=".txt,.md,.pdf,text/plain,text/markdown,application/pdf" required />
                  <p className="m-0 text-xs text-[var(--muted-foreground)]">
                    PDF fica guardado. A pesquisa lê texto e markdown.
                  </p>
                </div>
              )}
              {mode === "text" && (
                <div className="space-y-2 md:col-span-2">
                  <Label htmlFor="text">Texto</Label>
                  <textarea
                    id="text"
                    name="text"
                    required
                    className="min-h-28 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-sm"
                    placeholder="Cole o trecho que deve virar snapshot"
                  />
                </div>
              )}
              <div className="flex items-end gap-3">
                <Button type="submit" disabled={creating}>
                  {creating ? "Capturando…" : "Capturar snapshot"}
                </Button>
                <Button type="button" variant="outline" onClick={() => setShowCreate(false)}>
                  Cancelar
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {loading && <LoadingState label="Carregando fontes…" />}
      {!loading && error && <ErrorState title="Não foi possível carregar" description={error} />}
      {!loading && !error && items.length === 0 && (
        <EmptyState
          title="Nenhuma fonte cadastrada"
          description="Adicione a primeira origem para iniciar o monitoramento e capturar snapshots."
        />
      )}

      {!loading && !error && items.length > 0 && (
        <Card>
          <CardContent className="overflow-x-auto p-0">
            <table className="w-full min-w-[720px] border-collapse text-left text-sm">
              <thead>
                <tr className="border-b border-[var(--border)] text-xs text-[var(--muted-foreground)]">
                  <th className="px-5 py-3 font-semibold">Fonte</th>
                  <th className="px-5 py-3 font-semibold">Tipo</th>
                  <th className="px-5 py-3 font-semibold">URI</th>
                  <th className="px-5 py-3 font-semibold">Atualização</th>
                  <th className="px-5 py-3 font-semibold" />
                </tr>
              </thead>
              <tbody>
                {items.map((source) => (
                  <tr key={source.id} className="border-b border-[var(--border)]">
                    <td className="px-5 py-4 font-medium text-[var(--foreground)]">
                      {source.displayName}
                    </td>
                    <td className="px-5 py-4 text-[var(--muted-foreground)]">{source.kind}</td>
                    <td className="max-w-xs truncate px-5 py-4 text-[var(--muted-foreground)]">
                      {source.canonicalUri}
                    </td>
                    <td className="px-5 py-4 text-[var(--muted-foreground)]">
                      {formatUpdatedAt(source.updatedAt)}
                    </td>
                    <td className="px-5 py-4 text-right">
                      <div className="flex justify-end gap-3">
                        <Link
                          className="text-sm font-semibold text-[var(--primary)] hover:underline"
                          to={`/fontes/${source.id}`}
                        >
                          Abrir
                        </Link>
                        <button
                          type="button"
                          className="border-0 bg-transparent p-0 text-sm font-semibold text-[var(--destructive)]"
                          onClick={() => void handleDelete(source)}
                        >
                          Excluir
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="flex items-center justify-between border-t border-[var(--border)] px-5 py-3 text-xs text-[var(--muted-foreground)]">
              <span>
                1–{items.length} de {totalItems}
              </span>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
