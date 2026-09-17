import { Plus } from "lucide-react"
import { FormEvent, useEffect, useState } from "react"
import { Link } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import { createSource, listSources, type SourceListItem } from "@/lib/api/knowledge"

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
    const form = new FormData(event.currentTarget)

    try {
      await createSource({
        displayName: String(form.get("displayName") ?? ""),
        location: String(form.get("location") ?? ""),
        kind: String(form.get("kind") ?? "Web"),
      })
      toast.success("Fonte cadastrada")
      setShowCreate(false)
      event.currentTarget.reset()
      await loadSources()
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
            Cadastre, monitore e verifique as origens do conhecimento
          </p>
          <p className="mb-0 mt-3 text-xs text-[var(--muted-foreground)]">
            Também em Conhecimento:{" "}
            <Link className="font-semibold text-[var(--primary)] hover:underline" to="/claims">
              Revisão de evidências
            </Link>
          </p>
        </div>
        <Button type="button" onClick={() => setShowCreate((value) => !value)}>
          <Plus className="size-4" />
          Adicionar fonte
        </Button>
      </div>

      {showCreate && (
        <Card>
          <CardContent className="p-6">
            <form className="grid gap-4 md:grid-cols-2" onSubmit={handleCreate}>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="displayName">Nome</Label>
                <Input id="displayName" name="displayName" required placeholder="Relatório anual" />
              </div>
              <div className="space-y-2 md:col-span-2">
                <Label htmlFor="location">URI canônica</Label>
                <Input
                  id="location"
                  name="location"
                  required
                  placeholder="https://exemplo.com/documento"
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="kind">Tipo</Label>
                <select
                  id="kind"
                  name="kind"
                  defaultValue="Web"
                  className="h-12 w-full rounded-[10px] border border-[var(--border)] bg-[var(--card)] px-3 text-[13px]"
                >
                  <option value="Web">Web</option>
                  <option value="Upload">Upload</option>
                  <option value="Api">Api</option>
                </select>
              </div>
              <div className="flex items-end gap-3">
                <Button type="submit" disabled={creating}>
                  {creating ? "Salvando…" : "Salvar fonte"}
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
                      <Link
                        className="text-sm font-semibold text-[var(--primary)] hover:underline"
                        to={`/fontes/${source.id}`}
                      >
                        Abrir
                      </Link>
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
