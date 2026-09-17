import { useEffect, useState } from "react"
import { Link } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import {
  DEMO_EDITION_ID,
  DEMO_PRODUCT_ID,
  getEdition,
  readCatalogSelection,
  type EditionDetail,
} from "@/lib/api/catalog"
import {
  confirmPublication,
  createPublicationPackage,
  exportPublicationPackage,
  getPublicationPackage,
  type PublicationPackage,
} from "@/lib/api/publication"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function PublicacaoPage() {
  const [edition, setEdition] = useState<EditionDetail | null>(null)
  const [pkg, setPkg] = useState<PublicationPackage | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [acting, setActing] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [conflict, setConflict] = useState<string | null>(null)

  async function loadEdition() {
    setLoading(true)
    setError(null)
    try {
      const selection = readCatalogSelection()
      const productId = selection?.productId ?? DEMO_PRODUCT_ID
      const editionId = selection?.editionId ?? DEMO_EDITION_ID
      const result = await getEdition(productId, editionId)
      setEdition(result.data)
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 401
          ? "Faça login para gerenciar publicação."
          : requestError instanceof Error
            ? requestError.message
            : "Não foi possível carregar a edição.",
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadEdition()
  }, [])

  async function handleCreatePackage() {
    if (!edition) {
      return
    }
    setActing(true)
    setConflict(null)
    try {
      const result = await createPublicationPackage(edition.id)
      setPkg(result.data)
      setEtag(result.etag)
      toast.success("Pacote criado (ReadyForExport)")
    } catch (requestError) {
      toast.error(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível criar o pacote.",
      )
    } finally {
      setActing(false)
    }
  }

  async function handleExport() {
    if (!pkg) {
      return
    }
    setActing(true)
    try {
      const result = await exportPublicationPackage(pkg.id)
      setPkg(result.data)
      setEtag(result.etag)
      toast.success("Exportação gravada no blob store")
    } catch (requestError) {
      toast.error(
        requestError instanceof Error
          ? requestError.message
          : "Não foi possível exportar.",
      )
    } finally {
      setActing(false)
    }
  }

  async function handleConfirm() {
    if (!pkg || !etag) {
      return
    }
    setActing(true)
    setConflict(null)
    try {
      await confirmPublication(pkg.id, etag)
      const refreshed = await getPublicationPackage(pkg.id)
      setPkg(refreshed.data)
      setEtag(refreshed.etag)
      toast.success("Publicação confirmada (manual Kiwify)")
    } catch (requestError) {
      if (requestError instanceof ApiError && requestError.status === 412) {
        setConflict("O pacote mudou (412). Recarregue antes de confirmar.")
        toast.error("Conflito de versão (412)")
      } else if (requestError instanceof ApiError && requestError.status === 403) {
        toast.error("Sem capability publication.confirm")
      } else {
        toast.error(
          requestError instanceof Error
            ? requestError.message
            : "Não foi possível confirmar a publicação.",
        )
      }
    } finally {
      setActing(false)
    }
  }

  const canExport = pkg?.status === "ReadyForExport"
  const canConfirm = pkg?.status === "Exported"

  return (
    <div className="space-y-6">
      <header>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
          Publicação
        </p>
        <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Pacotes</h1>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Build determinístico, exportação e confirmação humana — passos separados.
        </p>
      </header>

      {loading && <LoadingState label="Carregando edição…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}
      {!loading && !error && !edition && (
        <EmptyState
          title="Nenhuma edição"
          description="Configure o currículo em Produtos antes de publicar."
        />
      )}

      {!loading && !error && edition && (
        <>
          <Card>
            <CardContent className="space-y-3 pt-6">
              <div>
                <h2 className="m-0 text-lg font-semibold">
                  {edition.productName} · {edition.name}
                </h2>
                <p className="mb-0 mt-1 text-sm text-[var(--muted-foreground)]">
                  {edition.curriculum.length} item(ns) no currículo ·{" "}
                  <Link className="underline" to="/produtos">
                    editar em Produtos
                  </Link>
                </p>
              </div>
              <Button
                type="button"
                disabled={acting || edition.curriculum.length === 0}
                onClick={() => void handleCreatePackage()}
              >
                {acting && !pkg ? "Criando…" : "Criar PublicationPackage"}
              </Button>
              {edition.curriculum.length === 0 && (
                <p className="m-0 text-sm text-[var(--muted-foreground)]">
                  Currículo vazio — adicione versões aprovadas no Catálogo.
                </p>
              )}
            </CardContent>
          </Card>

          {pkg && (
            <div className="grid gap-4 lg:grid-cols-2">
              <Card>
                <CardContent className="space-y-4 pt-6">
                  <div>
                    <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                      Exportar
                    </p>
                    <h3 className="m-0 mt-1 text-base font-semibold">
                      Pacote · {pkg.status}
                    </h3>
                    <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                      {pkg.id} · renderer {pkg.rendererVersion}
                    </p>
                  </div>
                  <pre className="max-h-48 overflow-auto whitespace-pre-wrap rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--muted)]/40 p-3 text-xs">
                    {pkg.manifestJson ?? "(sem manifesto)"}
                  </pre>
                  <Button
                    type="button"
                    disabled={acting || !canExport}
                    onClick={() => void handleExport()}
                  >
                    {canExport ? "Exportar para blob" : "Exportação indisponível"}
                  </Button>
                  {pkg.exportBlobSha256 && (
                    <p className="m-0 text-xs text-[var(--muted-foreground)]">
                      Blob {pkg.exportBlobSha256.slice(0, 16)}… · {pkg.exportBlobLength}{" "}
                      bytes · {formatWhen(pkg.updatedAt)}
                    </p>
                  )}
                </CardContent>
              </Card>

              <Card>
                <CardContent className="space-y-4 pt-6">
                  <div>
                    <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                      Confirmar publicação
                    </p>
                    <h3 className="m-0 mt-1 text-base font-semibold">
                      Confirmação humana
                    </h3>
                    <p className="mb-0 mt-1 text-sm text-[var(--muted-foreground)]">
                      Export ≠ publicado. Confirme somente após publicar manualmente no
                      Kiwify.
                    </p>
                  </div>
                  {conflict && (
                    <p className="m-0 rounded-[var(--radius-md)] bg-[var(--destructive)]/10 px-3 py-2 text-sm text-[var(--destructive)]">
                      {conflict}
                    </p>
                  )}
                  <Button
                    type="button"
                    variant={canConfirm ? "default" : "outline"}
                    disabled={acting || !canConfirm || !etag}
                    onClick={() => void handleConfirm()}
                  >
                    Confirmar publicação
                  </Button>
                  {pkg.status === "PublishedConfirmed" && (
                    <p className="m-0 text-sm text-[var(--muted-foreground)]">
                      Confirmado em{" "}
                      {pkg.confirmedAt ? formatWhen(pkg.confirmedAt) : "—"} por{" "}
                      {pkg.confirmedByUserId ?? "—"}.
                    </p>
                  )}
                  {!canConfirm && pkg.status !== "PublishedConfirmed" && (
                    <p className="m-0 text-xs text-[var(--muted-foreground)]">
                      Exporte o pacote antes de confirmar.
                    </p>
                  )}
                </CardContent>
              </Card>
            </div>
          )}
        </>
      )}
    </div>
  )
}
