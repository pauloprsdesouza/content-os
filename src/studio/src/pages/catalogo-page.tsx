import { FormEvent, useEffect, useState } from "react"
import { Link, useSearchParams } from "react-router"
import { toast } from "sonner"

import { EmptyState, ErrorState, LoadingState } from "@/components/page-states"
import { EditionPublication } from "@/components/edition-publication"
import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { ApiError } from "@/lib/api/client"
import {
  createProduct,
  deleteProduct,
  getEdition,
  listProducts,
  replaceCurriculum,
  writeCatalogSelection,
  type EditionDetail,
  type ProductListItem,
} from "@/lib/api/catalog"
import { getContentVersion, listContentUnits, type ContentUnitListItem } from "@/lib/api/content"

function formatWhen(value: string) {
  return new Intl.DateTimeFormat("pt-BR", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(new Date(value))
}

export function CatalogoPage() {
  const [searchParams] = useSearchParams()
  const requestedProductId = searchParams.get("produto")
  const [products, setProducts] = useState<ProductListItem[]>([])
  const [selectedProductId, setSelectedProductId] = useState<string | null>(null)
  const [edition, setEdition] = useState<EditionDetail | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [versionTitles, setVersionTitles] = useState<Record<string, string>>({})
  const [approvedUnits, setApprovedUnits] = useState<ContentUnitListItem[]>([])
  const [draftOrder, setDraftOrder] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [creating, setCreating] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [conflict, setConflict] = useState<string | null>(null)
  const [pendingCount, setPendingCount] = useState(0)

  async function load(preferProductId?: string) {
    setLoading(true)
    setError(null)
    try {
      const [productPage, unitsPage] = await Promise.all([
        listProducts(),
        listContentUnits(1, 100),
      ])
      setProducts(productPage.items)
      const product =
        productPage.items.find(
          (item) => item.id === (preferProductId ?? requestedProductId ?? selectedProductId),
        ) ?? productPage.items[0]
      setSelectedProductId(product?.id ?? null)

      if (!product?.editionId) {
        setEdition(null)
        setEtag(null)
        setDraftOrder([])
        setVersionTitles({})
        setApprovedUnits([])
        return
      }

      const editionResult = await getEdition(product.id, product.editionId)
      setEdition(editionResult.data)
      setEtag(editionResult.etag)
      setDraftOrder(editionResult.data.curriculum.map((item) => item.contentVersionId))
      setVersionTitles(await loadCurriculumTitles(editionResult.data.curriculum, unitsPage.items))
      writeCatalogSelection({
        productId: editionResult.data.productId,
        editionId: editionResult.data.id,
      })
      setApprovedUnits(
        unitsPage.items.filter(
          (unit) =>
            unit.latestVersionId &&
            unit.latestVersionStatus === "Approved" &&
            unit.productId === product.id,
        ),
      )
      setPendingCount(
        unitsPage.items.filter(
          (unit) =>
            unit.productId === product.id && unit.latestVersionStatus === "PendingHumanApproval",
        ).length,
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

  async function addApprovedAndSave() {
    if (!edition || !etag) {
      return
    }
    const missing = approvedUnits
      .map((unit) => unit.latestVersionId)
      .filter((id): id is string => Boolean(id))
      .filter((id) => !draftOrder.includes(id))
    if (missing.length === 0) {
      return
    }
    const next = [...draftOrder, ...missing]
    setDraftOrder(next)
    setSaving(true)
    setConflict(null)
    try {
      const result = await replaceCurriculum(edition.id, etag, next)
      setEtag(result.etag)
      toast.success("Versões adicionadas ao currículo")
      await load()
    } catch (requestError) {
      if (requestError instanceof ApiError && requestError.status === 412) {
        setConflict("A edição mudou (412). Recarregue antes de salvar.")
        toast.error("Conflito de versão (412)")
      } else {
        toast.error(
          requestError instanceof Error ? requestError.message : "Não foi possível atualizar o currículo.",
        )
      }
    } finally {
      setSaving(false)
    }
  }

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const form = event.currentTarget
    const data = new FormData(form)
    const name = String(data.get("name") ?? "").trim()
    if (!name) {
      toast.error("Dê um nome ao produto.")
      return
    }
    setCreating(true)
    try {
      const created = await createProduct({
        name,
        description: String(data.get("description") ?? "").trim() || undefined,
      })
      form.reset()
      toast.success("Produto criado.")
      await load(created.productId)
    } catch (requestError) {
      toast.error(requestError instanceof Error ? requestError.message : "Não foi possível criar o produto.")
    } finally {
      setCreating(false)
    }
  }

  async function handleDeleteProduct(product: ProductListItem) {
    if (!window.confirm(`Apagar o produto “${product.name}”? O currículo some com ele. Vendas e matrículas impedem a exclusão.`)) {
      return
    }
    try {
      await deleteProduct(product.id)
      toast.success("Produto apagado.")
      setSelectedProductId(null)
      await load()
    } catch (requestError) {
      toast.error(requestError instanceof Error ? requestError.message : "Não foi possível apagar o produto.")
    }
  }

  const missingApproved = approvedUnits.filter(
    (unit) => unit.latestVersionId && !draftOrder.includes(unit.latestVersionId),
  )
  const approvedByVersionId = new Map(
    approvedUnits
      .filter((unit) => unit.latestVersionId)
      .map((unit) => [unit.latestVersionId as string, unit]),
  )

  return (
    <div className="space-y-6">
      <header>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
          Edições
        </p>
        <h1 className="m-0 mt-1 text-2xl font-semibold tracking-tight">Produto e publicação</h1>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          O currículo só recebe versão aprovada. O arquivo baixado não publica na Kiwify.{" "}
          <Link className="font-semibold text-[var(--primary)]" to="/conteudo">
            Novo conteúdo
          </Link>
        </p>
      </header>

      {loading && <LoadingState label="Carregando catálogo…" />}
      {!loading && error && <ErrorState title="Erro ao carregar" description={error} />}
      {!loading && !error && products.length === 0 && (
        <EmptyState
          title="Nenhum produto"
          description="Crie o produto primeiro. Depois o assistente Novo conteúdo amarra cada peça a ele."
        />
      )}

      {!loading && !error && (
        <Card>
          <CardContent className="pt-6">
            <form className="grid gap-3 sm:grid-cols-[1fr_1fr_auto] sm:items-end" onSubmit={(event) => void handleCreate(event)}>
              <div className="space-y-1.5">
                <Label htmlFor="product-name">Novo produto</Label>
                <Input id="product-name" name="name" placeholder="Nome do produto" required />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="product-description">Descrição</Label>
                <Input id="product-description" name="description" placeholder="Opcional" />
              </div>
              <Button type="submit" disabled={creating}>
                {creating ? "Criando…" : "Criar produto"}
              </Button>
            </form>
          </CardContent>
        </Card>
      )}

      {!loading && !error && products.length > 0 && !edition && (
        <EmptyState
          title="Produto sem edição"
          description="Este produto não tem edição para montar o currículo."
        />
      )}

      {!loading && !error && edition && (
        <div className="grid gap-4 lg:grid-cols-[280px_1fr]">
          <Card>
            <CardContent className="space-y-2 pt-4">
              {products.map((product) => {
                const selected = product.id === selectedProductId
                return (
                  <div
                    key={product.id}
                    className={`flex items-start justify-between gap-2 rounded-[var(--radius-md)] px-3 py-2 text-sm ${
                      selected ? "bg-[var(--accent)]" : "border border-[var(--border)]"
                    }`}
                  >
                    <button
                      type="button"
                      className="min-w-0 flex-1 border-0 bg-transparent p-0 text-left"
                      onClick={() => {
                        setSelectedProductId(product.id)
                        void load(product.id)
                      }}
                    >
                      <div className="font-medium">{product.name}</div>
                      <div className="text-xs text-[var(--muted-foreground)]">
                        {product.description ?? "Sem descrição"} · {formatWhen(product.updatedAt)}
                      </div>
                    </button>
                    <Button
                      type="button"
                      size="sm"
                      variant="ghost"
                      onClick={() => void handleDeleteProduct(product)}
                    >
                      Excluir
                    </Button>
                  </div>
                )
              })}
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
                    O currículo entra depois da revisão. Só versões aprovadas deste produto.
                  </p>
                </div>
                {conflict && (
                  <p className="m-0 rounded-[var(--radius-md)] bg-[var(--destructive)]/10 px-3 py-2 text-sm text-[var(--destructive)]">
                    {conflict}
                  </p>
                )}
              </CardContent>
            </Card>

            {missingApproved.length > 0 && (
              <div className="flex flex-wrap items-center justify-between gap-3 rounded-[var(--radius-md)] border border-[var(--warning)] bg-[color-mix(in_srgb,var(--warning)_18%,white)] px-4 py-3">
                <div>
                  <p className="m-0 text-[11px] font-semibold uppercase tracking-[0.08em]">Próxima ação</p>
                  <p className="mb-0 mt-1 text-sm">
                    {missingApproved.length}{" "}
                    {missingApproved.length === 1
                      ? "versão aprovada ainda não está"
                      : "versões aprovadas ainda não estão"}{" "}
                    no currículo. O pacote só existe quando o currículo fechar.
                  </p>
                </div>
                <Button type="button" disabled={saving} onClick={() => void addApprovedAndSave()}>
                  {saving ? "Salvando…" : "Adicionar ao currículo"}
                </Button>
              </div>
            )}

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
                      const title = versionTitles[versionId] || unit?.title
                      return (
                        <li key={versionId} className="text-sm">
                          <div className="flex flex-wrap items-center gap-2">
                            <span className="font-medium">{title || "Peça sem título"}</span>
                            {!title && (
                              <span className="text-xs text-[var(--muted-foreground)]">{versionId}</span>
                            )}
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
                    Aprove versões em Revisão para montar o currículo.
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

            <EditionPublication edition={edition} />

            <div className="grid gap-4 lg:grid-cols-2">
              <Card>
                <CardContent className="space-y-2 pt-6">
                  <h3 className="m-0 text-base font-semibold">Conhecimento desta edição</h3>
                  <p className="m-0 text-sm text-[var(--muted-foreground)]">
                    {approvedUnits.length} versões aprovadas · {pendingCount} na fila de revisão.
                  </p>
                  <Link className="text-sm font-semibold text-[var(--primary)]" to="/revisao">
                    Ir para a revisão
                  </Link>
                </CardContent>
              </Card>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

async function loadCurriculumTitles(
  curriculum: { contentVersionId: string }[],
  units: ContentUnitListItem[],
) {
  const titleByUnit = new Map(units.map((unit) => [unit.id, unit.title]))
  const titles: Record<string, string> = {}
  await Promise.all(
    curriculum.map(async (item) => {
      try {
        const version = await getContentVersion(item.contentVersionId)
        const body = version.data.bodyMarkdown ?? ""
        const heading = body.match(/^#{1,3}\s+(.+)$/m)?.[1]?.replace(/[*_`]/g, "").trim()
        titles[item.contentVersionId] = titleByUnit.get(version.data.contentUnitId) || heading || ""
      } catch {
        titles[item.contentVersionId] = ""
      }
    }),
  )
  return titles
}
