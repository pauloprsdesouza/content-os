import { useEffect, useState } from "react"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import {
  DEMO_EDITION_ID,
  DEMO_PRODUCT_ID,
  getEdition,
  listProducts,
  replaceCurriculum,
  writeCatalogSelection,
  type EditionDetail,
  type ProductListItem,
} from "@/lib/api/catalog"
import { listContentUnits, type ContentUnitListItem } from "@/lib/api/content"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function CatalogoPage() {
  const [products, setProducts] = useState<ProductListItem[]>([])
  const [edition, setEdition] = useState<EditionDetail | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [approvedUnits, setApprovedUnits] = useState<ContentUnitListItem[]>([])
  const [draftOrder, setDraftOrder] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [conflict, setConflict] = useState<string | null>(null)

  async function load() {
    setLoading(true)
    setError(null)
    try {
      const [productPage, unitsPage] = await Promise.all([
        listProducts(),
        listContentUnits(),
      ])
      setProducts(productPage.items)

      const productId = productPage.items[0]?.id ?? DEMO_PRODUCT_ID
      const editionId = DEMO_EDITION_ID

      const editionResult = await getEdition(productId, editionId)
      setEdition(editionResult.data)
      setEtag(editionResult.etag)
      setDraftOrder(
        editionResult.data.curriculum.map((item) => item.contentVersionId),
      )
      writeCatalogSelection({
        productId: editionResult.data.productId,
        editionId: editionResult.data.id,
      })

      setApprovedUnits(
        unitsPage.items.filter(
          (unit) =>
            unit.latestVersionId && unit.latestVersionStatus === "Approved",
        ),
      )
    } catch (requestError) {
      setError(
        requestError instanceof ApiError && requestError.status === 401
          ? "Faça login para gerenciar o catálogo."
          : requestError instanceof Error
            ? requestError.message
            : "Não foi possível carregar o catálogo.",
      )
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  function addToCurriculum(versionId: string) {
    setDraftOrder((current) =>
      current.includes(versionId) ? current : [...current, versionId],
    )
  }

  function removeFromCurriculum(versionId: string) {
    setDraftOrder((current) => current.filter((id) => id !== versionId))
  }

  function move(versionId: string, direction: -1 | 1) {
    setDraftOrder((current) => {
      const index = current.indexOf(versionId)
      if (index < 0) {
        return current
      }
      const target = index + direction
      if (target < 0 || target >= current.length) {
        return current
      }
      const next = [...current]
      const [item] = next.splice(index, 1)
      next.splice(target, 0, item)
      return next
    })
  }

  async function handleSave() {
    if (!edition || !etag) {
      return
    }

    setSaving(true)
    setConflict(null)
    try {
      const result = await replaceCurriculum(edition.id, etag, draftOrder)
      setEtag(result.etag)
      toast.success("Currículo atualizado")
      await load()
    } catch (requestError) {
      if (requestError instanceof ApiError && requestError.status === 412) {
        setConflict("A edição mudou (412). Recarregue antes de salvar.")
        toast.error("Conflito de versão (412)")
      } else {
        toast.error(
          requestError instanceof Error
            ? requestError.message
            : "Não foi possível salvar o currículo.",
        )
      }
    } finally {
      setSaving(false)
    }
  }

  const approvedByVersionId = new Map(
    approvedUnits
      .filter((unit) => unit.latestVersionId)
      .map((unit) => [unit.latestVersionId as string, unit]),
  )

  return (
    <div className="space-y-6">
      <header>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
          Catálogo
        </p>
        <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Produtos</h1>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Produto, edição e currículo com versões aprovadas explícitas (nunca “latest”).
        </p>
      </header>

      {loading && <LoadingState label="Carregando catálogo…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}
      {!loading && !error && products.length === 0 && (
        <EmptyState
          title="Nenhum produto"
          description="Rode as migrations e reinicie a API em Development para seedar o produto demo."
        />
      )}

      {!loading && !error && edition && (
        <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
          <Card>
            <CardContent className="space-y-2 pt-4">
              {products.map((product) => (
                <div
                  key={product.id}
                  className="rounded-[var(--radius-md)] bg-[var(--accent)] px-3 py-2 text-sm"
                >
                  <div className="font-medium">{product.name}</div>
                  <div className="text-xs text-[var(--muted-foreground)]">
                    {product.description ?? "Sem descrição"} ·{" "}
                    {formatWhen(product.updatedAt)}
                  </div>
                </div>
              ))}
            </CardContent>
          </Card>

          <div className="space-y-4">
            <Card>
              <CardContent className="space-y-3 pt-6">
                <div>
                  <h2 className="m-0 text-lg font-semibold">
                    {edition.productName} · {edition.name}
                  </h2>
                  <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                    Edição {edition.id} · ETag {etag ?? "—"}
                  </p>
                </div>
                {conflict && (
                  <p className="m-0 rounded-[var(--radius-md)] bg-[var(--destructive)]/10 px-3 py-2 text-sm text-[var(--destructive)]">
                    {conflict}
                  </p>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardContent className="space-y-4 pt-6">
                <div className="flex items-center justify-between gap-3">
                  <h3 className="m-0 text-base font-semibold">Currículo ordenado</h3>
                  <Button type="button" disabled={saving} onClick={() => void handleSave()}>
                    {saving ? "Salvando…" : "Salvar currículo"}
                  </Button>
                </div>

                {draftOrder.length === 0 ? (
                  <p className="m-0 text-sm text-[var(--muted-foreground)]">
                    Nenhum ContentVersion no currículo. Adicione versões aprovadas à direita.
                  </p>
                ) : (
                  <ol className="m-0 list-decimal space-y-2 pl-5">
                    {draftOrder.map((versionId, index) => {
                      const unit = approvedByVersionId.get(versionId)
                      return (
                        <li key={versionId} className="text-sm">
                          <div className="flex flex-wrap items-center gap-2">
                            <span className="font-medium">
                              {unit?.title ?? versionId.slice(0, 8)}
                            </span>
                            <span className="text-xs text-[var(--muted-foreground)]">
                              {versionId}
                            </span>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              disabled={index === 0}
                              onClick={() => move(versionId, -1)}
                            >
                              Subir
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="outline"
                              disabled={index === draftOrder.length - 1}
                              onClick={() => move(versionId, 1)}
                            >
                              Descer
                            </Button>
                            <Button
                              type="button"
                              size="sm"
                              variant="ghost"
                              onClick={() => removeFromCurriculum(versionId)}
                            >
                              Remover
                            </Button>
                          </div>
                        </li>
                      )
                    })}
                  </ol>
                )}
              </CardContent>
            </Card>

            <Card>
              <CardContent className="space-y-3 pt-6">
                <h3 className="m-0 text-base font-semibold">Versões aprovadas</h3>
                {approvedUnits.length === 0 ? (
                  <p className="m-0 text-sm text-[var(--muted-foreground)]">
                    Aprove ContentVersions em Conteúdo para montar o currículo.
                  </p>
                ) : (
                  <ul className="m-0 space-y-2 p-0">
                    {approvedUnits.map((unit) => {
                      const versionId = unit.latestVersionId!
                      const alreadyIn = draftOrder.includes(versionId)
                      return (
                        <li
                          key={unit.id}
                          className="flex items-center justify-between gap-3 rounded-[var(--radius-md)] border border-[var(--border)] px-3 py-2 text-sm"
                        >
                          <div>
                            <div className="font-medium">{unit.title}</div>
                            <div className="text-xs text-[var(--muted-foreground)]">
                              {versionId}
                            </div>
                          </div>
                          <Button
                            type="button"
                            size="sm"
                            variant="outline"
                            disabled={alreadyIn}
                            onClick={() => addToCurriculum(versionId)}
                          >
                            {alreadyIn ? "No currículo" : "Adicionar"}
                          </Button>
                        </li>
                      )
                    })}
                  </ul>
                )}
              </CardContent>
            </Card>
          </div>
        </div>
      )}
    </div>
  )
}
