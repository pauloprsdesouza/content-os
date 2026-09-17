import { FormEvent, useEffect, useState } from "react"
import { Link, useParams } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import {
  createSnapshot,
  getSource,
  listSnapshots,
  type SnapshotItem,
  type SourceDetail,
} from "@/lib/api/knowledge"

function toBase64(text: string) {
  const bytes = new TextEncoder().encode(text)
  let binary = ""
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte)
  })
  return btoa(binary)
}

export function SourceDetailPage() {
  const { sourceId = "" } = useParams()
  const [source, setSource] = useState<SourceDetail | null>(null)
  const [snapshots, setSnapshots] = useState<SnapshotItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [creating, setCreating] = useState(false)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const [detail, page] = await Promise.all([getSource(sourceId), listSnapshots(sourceId)])
      setSource(detail)
      setSnapshots(page.items)
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível carregar a fonte.",
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    if (sourceId) {
      void load()
    }
  }, [sourceId])

  async function handleCreateSnapshot(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setCreating(true)
    const form = new FormData(event.currentTarget)
    const content = String(form.get("content") ?? "")

    try {
      await createSnapshot(sourceId, {
        contentBase64: toBase64(content),
        mediaType: "text/plain",
      })
      toast.success("Snapshot capturado")
      event.currentTarget.reset()
      await load()
    } catch (requestError) {
      toast.error(
        requestError instanceof ApiError
          ? requestError.message
          : "Falha ao capturar snapshot.",
      )
    } finally {
      setCreating(false)
    }
  }

  if (loading) {
    return <LoadingState label="Carregando fonte…" />
  }

  if (error || !source) {
    return (
      <ErrorState
        title="Fonte indisponível"
        description={error ?? "Fonte não encontrada."}
      />
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <Link className="text-xs font-semibold text-[var(--primary)] hover:underline" to="/fontes">
          ← Voltar para fontes
        </Link>
        <h2 className="mb-0 mt-3 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
          {source.displayName}
        </h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">{source.canonicalUri}</p>
        <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">Tipo: {source.kind}</p>
      </div>

      <Card>
        <CardContent className="space-y-4 p-6">
          <h3 className="m-0 text-lg font-semibold">Capturar snapshot</h3>
          <form className="space-y-3" onSubmit={handleCreateSnapshot}>
            <div className="space-y-2">
              <Label htmlFor="content">Conteúdo textual</Label>
              <textarea
                id="content"
                name="content"
                required
                className="min-h-28 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-sm"
                placeholder="Cole o conteúdo capturado da fonte…"
              />
            </div>
            <Button type="submit" disabled={creating}>
              {creating ? "Capturando…" : "Criar snapshot"}
            </Button>
          </form>
        </CardContent>
      </Card>

      {snapshots.length === 0 ? (
        <EmptyState
          title="Nenhum snapshot"
          description="Capture o primeiro snapshot imutável para vincular evidências e claims."
        />
      ) : (
        <Card>
          <CardContent className="space-y-3 p-6">
            <h3 className="m-0 text-lg font-semibold">Snapshots</h3>
            <ul className="m-0 list-none space-y-3 p-0">
              {snapshots.map((snapshot) => (
                <li
                  key={snapshot.id}
                  className="rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-4 py-3 text-sm"
                >
                  <p className="m-0 font-medium">{snapshot.contentHash.slice(0, 16)}…</p>
                  <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                    {snapshot.mediaType} · {snapshot.byteLength} bytes ·{" "}
                    {new Date(snapshot.capturedAt).toLocaleString("pt-BR")}
                  </p>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      )}
    </div>
  )
}
