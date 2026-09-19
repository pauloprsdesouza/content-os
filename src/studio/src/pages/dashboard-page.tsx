import { useEffect, useState } from "react"
import { Link } from "react-router"

import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import { getEdition, listProducts, type ProductListItem } from "@/lib/api/catalog"
import { listContentUnits, type ContentUnitListItem } from "@/lib/api/content"
import { decisionCount, getDashboardSummary, type DashboardSummary } from "@/lib/api/dashboard"

type EditionRow = {
  product: ProductListItem
  missingApproved: number
  curriculumCount: number
}

export function DashboardPage() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null)
  const [rows, setRows] = useState<EditionRow[]>([])
  const [ready, setReady] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    async function load() {
      try {
        const [payload, products, units] = await Promise.all([
          getDashboardSummary(),
          listProducts(),
          listContentUnits(1, 100),
        ])
        const editionRows = await Promise.all(
          products.items.slice(0, 8).map((product) => loadEditionRow(product, units.items)),
        )
        if (!cancelled) {
          setSummary(payload)
          setRows(editionRows)
          setReady(true)
        }
      } catch (requestError) {
        if (!cancelled) {
          setError(
            requestError instanceof ApiError ? requestError.message : "Falha ao carregar o trabalho de hoje.",
          )
          setReady(true)
        }
      }
    }
    void load()
    return () => {
      cancelled = true
    }
  }, [])

  const pending = summary ? decisionCount(summary) : null
  const cards = summary
    ? [
        summary.claimsPendingReview > 0
          ? {
              kicker: `${summary.claimsPendingReview} afirmações`,
              title: "Revisar evidências",
              body: "Cada afirmação mostra a evidência antes do botão. Aprovar não publica.",
              href: "/revisao",
              cta: "Revisar",
            }
          : null,
        summary.contentVersionsPendingApproval > 0
          ? {
              kicker: `${summary.contentVersionsPendingApproval} versões`,
              title: "Aprovar conteúdo",
              body: "O agente recomendou. A versão só entra no currículo depois da sua aprovação.",
              href: "/revisao?aba=conteudo",
              cta: "Abrir",
            }
          : null,
        summary.publicationPackagesReady > 0
          ? {
              kicker: `${summary.publicationPackagesReady} pacotes`,
              title: "Confirmar publicação",
              body: "Já exportados. A venda só existe depois que você confirmar a publicação na Kiwify.",
              href: "/edicoes",
              cta: "Confirmar",
            }
          : null,
      ].filter((item): item is NonNullable<typeof item> => item !== null)
    : []

  return (
    <div className="space-y-6">
      <section>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--primary)]">
          Trabalho de hoje
        </p>
        <h1 className="m-0 mt-2 text-[30px] font-bold tracking-[-0.03em]">
          {pending === null ? "Carregando decisões…" : pending === 0 ? "Nada espera você" : `${pending} ${pending === 1 ? "decisão espera" : "decisões esperam"} você`}
        </h1>
        <p className="mb-0 mt-2 max-w-2xl text-sm text-[var(--muted-foreground)]">
          Nada segue sem intenção humana. A IA propôs; aprovar, rejeitar e confirmar publicação continuam com você.
        </p>
        {error && <p className="mt-2 text-sm text-[var(--destructive)]">{error}</p>}
      </section>

      {summary && cards.length > 0 && (
        <section className="grid gap-4 lg:grid-cols-3">
          {cards.map((card) => (
            <Card key={card.href}>
              <CardContent className="flex h-full flex-col gap-3 p-5">
                <p className="m-0 text-[11px] font-semibold uppercase tracking-[0.08em] text-[var(--primary)]">
                  {card.kicker}
                </p>
                <h2 className="m-0 text-lg font-semibold">{card.title}</h2>
                <p className="m-0 flex-1 text-sm text-[var(--muted-foreground)]">{card.body}</p>
                <Link
                  className="inline-flex h-10 w-fit items-center rounded-[10px] bg-[var(--primary)] px-4 text-[13px] font-semibold text-white no-underline"
                  to={card.href}
                >
                  {card.cta}
                </Link>
              </CardContent>
            </Card>
          ))}
        </section>
      )}

      {summary && summary.researchJobsActive > 0 && (
        <p className="m-0 text-sm text-[var(--muted-foreground)]">
          {summary.researchJobsActive}{" "}
          {summary.researchJobsActive === 1 ? "pesquisa em execução" : "pesquisas em execução"}.{" "}
          <Link className="font-semibold text-[var(--primary)]" to="/pesquisa">
            Acompanhar
          </Link>
        </p>
      )}

      <section className="space-y-3">
        <h2 className="m-0 text-sm font-semibold">Continuar por edição</h2>
        {ready && rows.length === 0 && !error && (
          <Card>
            <CardContent className="p-5 text-sm text-[var(--muted-foreground)]">
              Nenhuma edição ainda.{" "}
              <Link className="font-semibold text-[var(--primary)]" to="/edicoes">
                Criar a primeira
              </Link>
            </CardContent>
          </Card>
        )}
        {rows.map((row) => {
          const status =
            row.missingApproved > 0
              ? "Currículo incompleto"
              : row.curriculumCount === 0
                ? "Currículo vazio"
                : "Currículo fechado"
          return (
            <Card key={row.product.id}>
              <CardContent className="flex flex-wrap items-center justify-between gap-3 p-4">
                <div>
                  <p className="m-0 text-sm font-semibold">{row.product.name}</p>
                  <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                    {row.curriculumCount} no currículo
                    {row.missingApproved > 0
                      ? ` · ${row.missingApproved} aprovadas fora do currículo`
                      : ""}
                  </p>
                </div>
                <div className="flex items-center gap-4">
                  <span className="rounded-full bg-[var(--warning)] px-2 py-1 text-[11px] font-semibold text-[var(--foreground)]">
                    {status}
                  </span>
                  <Link
                    className="text-sm font-semibold text-[var(--primary)] no-underline"
                    to={`/edicoes?produto=${row.product.id}`}
                  >
                    Continuar
                  </Link>
                </div>
              </CardContent>
            </Card>
          )
        })}
      </section>
    </div>
  )
}

async function loadEditionRow(
  product: ProductListItem,
  units: ContentUnitListItem[],
): Promise<EditionRow> {
  if (!product.editionId) {
    return { product, missingApproved: 0, curriculumCount: 0 }
  }
  try {
    const edition = await getEdition(product.id, product.editionId)
    const placed = new Set(edition.data.curriculum.map((item) => item.contentVersionId))
    const missingApproved = units.filter(
      (unit) =>
        unit.productId === product.id &&
        unit.latestVersionStatus === "Approved" &&
        unit.latestVersionId &&
        !placed.has(unit.latestVersionId),
    ).length
    return {
      product,
      missingApproved,
      curriculumCount: edition.data.curriculum.length,
    }
  } catch {
    return { product, missingApproved: 0, curriculumCount: 0 }
  }
}
