import { useEffect, useState } from "react"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { ApiError } from "@/lib/api/client"
import type { EditionDetail } from "@/lib/api/catalog"
import { getContentVersion, listContentUnits } from "@/lib/api/content"
import {
  confirmPublication,
  createPublicationPackage,
  exportPublicationPackage,
  getPublicationPackage,
  listPublicationPackages,
  type PublicationPackage,
} from "@/lib/api/publication"

type ExportPiece = {
  position: number
  title: string
  body: string
}

export function EditionPublication({ edition }: { edition: EditionDetail }) {
  const [pkg, setPkg] = useState<PublicationPackage | null>(null)
  const [etag, setEtag] = useState<string | null>(null)
  const [pieces, setPieces] = useState<ExportPiece[]>([])
  const [piecesError, setPiecesError] = useState<string | null>(null)
  const [loading, setLoading] = useState(true)
  const [acting, setActing] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    setError(null)
    setPiecesError(null)

    async function load() {
      const [packageResult, pieceResult] = await Promise.allSettled([
        loadCurrentPackage(edition.id),
        loadPieces(edition),
      ])
      if (cancelled) {
        return
      }
      if (packageResult.status === "fulfilled") {
        setPkg(packageResult.value?.data ?? null)
        setEtag(packageResult.value?.etag ?? null)
      } else {
        setPkg(null)
        setEtag(null)
      }
      if (pieceResult.status === "fulfilled") {
        setPieces(pieceResult.value)
      } else {
        setPieces([])
        setPiecesError(
          pieceResult.reason instanceof Error
            ? pieceResult.reason.message
            : "Não foi possível ler o currículo.",
        )
      }
    }

    void load().finally(() => {
      if (!cancelled) {
        setLoading(false)
      }
    })
    return () => {
      cancelled = true
    }
  }, [edition])

  const confirmed = pkg?.status === "PublishedConfirmed"
  const exported = pkg?.status === "Exported" || confirmed
  const canDownload = edition.curriculum.length > 0 && pieces.length > 0

  async function handleDownload() {
    if (!canDownload) {
      return
    }
    setActing(true)
    setError(null)
    try {
      let current = pkg
      let currentEtag = etag
      if (!current) {
        const created = await createPublicationPackage(edition.id)
        current = created.data
        currentEtag = created.etag
      }
      if (current.status === "ReadyForExport") {
        const exportedPackage = await exportPublicationPackage(current.id)
        current = exportedPackage.data
        currentEtag = exportedPackage.etag
      }
      downloadText(exportFileName(edition), buildExportMarkdown(edition, pieces))
      setPkg(current)
      setEtag(currentEtag)
      toast.success("Arquivo baixado. Publique na Kiwify e depois confirme aqui.")
    } catch (requestError) {
      const message = exportErrorMessage(requestError)
      setError(message)
      toast.error(message)
    } finally {
      setActing(false)
    }
  }

  async function handleConfirm() {
    if (!pkg || !etag || pkg.status !== "Exported") {
      return
    }
    setActing(true)
    setError(null)
    try {
      await confirmPublication(pkg.id, etag)
      const refreshed = await getPublicationPackage(pkg.id)
      setPkg(refreshed.data)
      setEtag(refreshed.etag)
      toast.success("Publicação confirmada")
    } catch (requestError) {
      const message =
        requestError instanceof ApiError && requestError.status === 412
          ? "O pacote mudou. Baixe de novo antes de confirmar."
          : requestError instanceof ApiError && requestError.status === 403
            ? "Sem permissão para confirmar a publicação."
            : requestError instanceof Error
              ? requestError.message
              : "Não foi possível confirmar a publicação."
      setError(message)
      toast.error(message)
    } finally {
      setActing(false)
    }
  }

  return (
    <Card>
      <CardContent className="space-y-4 pt-6">
        <div>
          <h3 className="m-0 text-base font-semibold">O que será exportado</h3>
          <p className="mb-0 mt-1 text-sm text-[var(--muted-foreground)]">
            Um arquivo Markdown com o texto de cada peça, na ordem do currículo. Baixar não publica na Kiwify.
          </p>
        </div>

        {loading && <p className="m-0 text-sm text-[var(--muted-foreground)]">Lendo o currículo…</p>}
        {piecesError && <p className="m-0 text-sm text-[var(--destructive)]">{piecesError}</p>}
        {!loading && edition.curriculum.length === 0 && (
          <p className="m-0 text-sm text-[var(--muted-foreground)]">
            O currículo está vazio. Só entra versão aprovada, e o arquivo só existe depois disso.
          </p>
        )}
        {!loading && pieces.length > 0 && (
          <ol className="m-0 list-none space-y-3 p-0">
            {pieces.map((piece) => (
              <li key={`${piece.position}-${piece.title}`} className="rounded-[var(--radius-md)] border border-[var(--border)] px-3 py-3">
                <p className="m-0 text-sm font-semibold">
                  {String(piece.position).padStart(2, "0")}. {piece.title}
                </p>
                <p className="mb-0 mt-1 text-sm text-[var(--muted-foreground)]">{excerpt(piece.body)}</p>
              </li>
            ))}
          </ol>
        )}

        {error && (
          <p className="m-0 rounded-[var(--radius-md)] bg-[var(--destructive)]/10 px-3 py-2 text-sm text-[var(--destructive)]">
            {error}
          </p>
        )}

        <div className="flex flex-wrap gap-2">
          <Button type="button" disabled={acting || loading || !canDownload} onClick={() => void handleDownload()}>
            {acting ? "Preparando arquivo…" : exported ? "Baixar de novo" : "Baixar pacote"}
          </Button>
          {pkg?.status === "Exported" && (
            <Button type="button" variant="outline" disabled={acting || !etag} onClick={() => void handleConfirm()}>
              Confirmar publicação na Kiwify
            </Button>
          )}
        </div>
        {confirmed && (
          <p className="m-0 text-sm text-[var(--foreground)]">
            Publicação confirmada. A venda só aparece em Desempenho depois da reconciliação.
          </p>
        )}
        {pkg?.status === "Exported" && (
          <p className="m-0 text-sm text-[var(--muted-foreground)]">
            O arquivo já foi gerado. Confirme só depois de publicar esse conteúdo na Kiwify.
          </p>
        )}
      </CardContent>
    </Card>
  )
}

async function loadCurrentPackage(editionId: string) {
  try {
    const packages = await listPublicationPackages(editionId)
    const current = pickPackage(packages)
    if (!current) {
      return null
    }
    return await getPublicationPackage(current.id)
  } catch {
    return null
  }
}

async function loadPieces(edition: EditionDetail): Promise<ExportPiece[]> {
  if (edition.curriculum.length === 0) {
    return []
  }
  const ordered = [...edition.curriculum].sort((left, right) => left.position - right.position)
  const versions = await Promise.all(
    ordered.map(async (item) => (await getContentVersion(item.contentVersionId)).data),
  )
  const titleByUnit = await loadUnitTitles(
    edition.productId,
    versions.map((version) => version.contentUnitId),
  )
  return versions.map((version, index) => {
    const body = version.bodyMarkdown ?? ""
    return {
      position: index + 1,
      title: titleByUnit.get(version.contentUnitId) ?? headingTitle(body) ?? `Peça ${index + 1}`,
      body,
    }
  })
}

async function loadUnitTitles(productId: string, unitIds: string[]) {
  const needed = new Set(unitIds)
  const scoped = await listContentUnits(1, 100, productId)
  const found = new Map(scoped.items.map((unit) => [unit.id, unit.title]))
  if ([...needed].every((id) => found.has(id))) {
    return found
  }
  const all = await listContentUnits(1, 100)
  for (const unit of all.items) {
    if (needed.has(unit.id) && !found.has(unit.id)) {
      found.set(unit.id, unit.title)
    }
  }
  return found
}

function headingTitle(body: string) {
  const heading = body.match(/^#{1,3}\s+(.+)$/m)
  const title = heading?.[1]?.replace(/[*_`]/g, "").trim()
  return title || null
}

function pickPackage(packages: PublicationPackage[]) {
  return (
    packages.find((item) => item.status === "Exported") ??
    packages.find((item) => item.status === "ReadyForExport") ??
    packages.find((item) => item.status === "PublishedConfirmed") ??
    packages[0] ??
    null
  )
}

function excerpt(body: string) {
  const withoutHeading = body.replace(/^#{1,3}\s+.+$/m, "")
  const text = withoutHeading.replace(/\s+/g, " ").trim()
  if (!text) {
    return "Sem texto nesta peça."
  }
  return text.length > 220 ? `${text.slice(0, 220)}…` : text
}

function buildExportMarkdown(edition: EditionDetail, pieces: ExportPiece[]) {
  const lines = [
    `# ${edition.productName} — ${edition.name}`,
    "",
    "Exportação do Content Studio para publicação manual.",
    "Baixar este arquivo não publica na Kiwify. Confirme no Studio só depois de publicar lá.",
    "",
  ]
  for (const piece of pieces) {
    lines.push(`## ${String(piece.position).padStart(2, "0")}. ${piece.title}`, "", piece.body.trim() || "(sem texto)", "")
  }
  return lines.join("\n")
}

function exportFileName(edition: EditionDetail) {
  const base = `${edition.productName}-${edition.name}`
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[^a-zA-Z0-9]+/g, "-")
    .replace(/^-|-$/g, "")
    .toLowerCase()
  return `${base || "pacote"}.md`
}

function downloadText(filename: string, text: string) {
  const blob = new Blob([text], { type: "text/markdown;charset=utf-8" })
  const url = URL.createObjectURL(blob)
  const link = document.createElement("a")
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}

function exportErrorMessage(error: unknown) {
  if (error instanceof ApiError && error.status === 409) {
    return "O currículo mudou ou o pacote não pode ser exportado. Recarregue a edição."
  }
  if (error instanceof ApiError && error.status === 404) {
    return "A edição ou o pacote não foi encontrado."
  }
  return error instanceof Error ? error.message : "Não foi possível baixar o pacote."
}
