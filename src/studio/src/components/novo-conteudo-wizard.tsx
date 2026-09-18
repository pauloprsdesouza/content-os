import { FormEvent, useEffect, useState } from "react"
import { Link } from "react-router"
import { toast } from "sonner"

import { TopicChoiceList } from "@/components/topic-choice-list"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  getTopicDiscovery,
  listTopicAreas,
  selectDiscoveredTopics,
  startTopicDiscovery,
  type TopicArea,
  type TopicProposal,
} from "@/lib/api/editorial"
import { createContentUnit } from "@/lib/api/content"
import { listProducts, type ProductListItem } from "@/lib/api/catalog"
import { captureSource, uploadSourceFile } from "@/lib/api/knowledge"
import { watchOperation } from "@/lib/api/operations"
import { contentFormats } from "@/lib/task-status"

type Origin = "materials" | "topics"
type MaterialMode = "web" | "file" | "text"
type WizardStep = "product" | "format" | "origin" | "materials" | "topics" | "choose"

const pipeline = [
  { id: "product", label: "Produto" },
  { id: "format", label: "Formato" },
  { id: "origin", label: "Origem" },
  { id: "draft", label: "Rascunho" },
  { id: "review", label: "Revisão" },
] as const

function pipelineIndex(step: WizardStep) {
  if (step === "product") return 0
  if (step === "format") return 1
  if (step === "origin" || step === "materials" || step === "topics" || step === "choose") return 2
  return 3
}

export function NovoConteudoWizard({ onCreated }: { onCreated: () => Promise<void> }) {
  const [step, setStep] = useState<WizardStep>("product")
  const [products, setProducts] = useState<ProductListItem[]>([])
  const [productId, setProductId] = useState("")
  const [format, setFormat] = useState<string>("")
  const [origin, setOrigin] = useState<Origin>("materials")
  const [materialMode, setMaterialMode] = useState<MaterialMode>("web")
  const [title, setTitle] = useState("")
  const [displayName, setDisplayName] = useState("")
  const [location, setLocation] = useState("")
  const [text, setText] = useState("")
  const [file, setFile] = useState<File | null>(null)
  const [fields, setFields] = useState<TopicArea[]>([])
  const [subfields, setSubfields] = useState<TopicArea[]>([])
  const [fieldId, setFieldId] = useState("")
  const [subfieldId, setSubfieldId] = useState("")
  const [windowDays, setWindowDays] = useState(30)
  const [proposals, setProposals] = useState<TopicProposal[]>([])
  const [discoveryId, setDiscoveryId] = useState<string | null>(null)
  const [selected, setSelected] = useState<string[]>([])
  const [busy, setBusy] = useState(false)
  const [statusNote, setStatusNote] = useState<string | null>(null)

  useEffect(() => {
    void listProducts()
      .then((page) => setProducts(page.items))
      .catch(() => toast.error("Não foi possível carregar os produtos."))
  }, [])

  useEffect(() => {
    if (step !== "topics") {
      return
    }
    void listTopicAreas()
      .then(setFields)
      .catch(() => toast.error("Não foi possível carregar as áreas."))
  }, [step])

  useEffect(() => {
    if (!fieldId) {
      setSubfields([])
      setSubfieldId("")
      return
    }
    void listTopicAreas(fieldId)
      .then(setSubfields)
      .catch(() => setSubfields([]))
  }, [fieldId])

  async function handleMaterials(event: FormEvent) {
    event.preventDefault()
    if (!format || !title.trim() || !productId) {
      return
    }
    setBusy(true)
    try {
      const name = displayName.trim() || title.trim()
      const captured =
        materialMode === "file"
          ? await uploadSourceFile(name, file as File)
          : await captureSource({
              mode: materialMode === "web" ? "web" : "text",
              displayName: name,
              location: materialMode === "web" ? location.trim() : undefined,
              text: materialMode === "text" ? text : undefined,
            })
      await createContentUnit({
        title: title.trim(),
        format,
        citationContentHashes: [captured.contentHash],
        productId,
      })
      toast.success("Escrevendo o rascunho. Quando ficar pronto, revise em Conteúdo.")
      setStep("product")
      setTitle("")
      await onCreated()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível criar o conteúdo.")
    } finally {
      setBusy(false)
    }
  }

  async function handleSearchTopics(event: FormEvent) {
    event.preventDefault()
    const area = subfields.find((item) => item.id === subfieldId) ?? fields.find((item) => item.id === fieldId)
    if (!format || !area) {
      return
    }
    setBusy(true)
    setStatusNote("coletando temas")
    try {
      const started = await startTopicDiscovery({
        format,
        areaId: area.id,
        areaName: area.name,
        areaIsSubfield: Boolean(subfieldId),
        windowDays,
      })
      setDiscoveryId(started.discoveryId)
      if (started.isEmpty || !started.operationId) {
        setProposals([])
        setStatusNote("escolha os temas")
        setStep("choose")
        return
      }
      await watchOperation(started.operationId, () => undefined)
      const discovery = await getTopicDiscovery(started.discoveryId)
      setProposals(discovery.proposals)
      setStatusNote("escolha os temas")
      setStep("choose")
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível buscar temas.")
      setStatusNote(null)
    } finally {
      setBusy(false)
    }
  }

  async function handleSelect() {
    if (!discoveryId) {
      return
    }
    setBusy(true)
    setStatusNote("escrevendo")
    try {
      await selectDiscoveredTopics(discoveryId, selected, productId)
      toast.success("Escrevendo o rascunho. Quando ficar pronto, revise em Conteúdo.")
      setStep("product")
      setSelected([])
      setProposals([])
      await onCreated()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível escrever o rascunho.")
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
          Novo conteúdo
        </p>
        <ol className="mt-3 flex flex-wrap gap-2 p-0">
          {pipeline.map((item, index) => {
            const current = pipelineIndex(step)
            const done = index < current
            const active = index === current
            return (
              <li
                key={item.id}
                className={`rounded-full px-3 py-1 text-xs font-medium ${
                  active
                    ? "bg-[var(--primary)] text-[var(--primary-foreground)]"
                    : done
                      ? "bg-[var(--accent)] text-[var(--foreground)]"
                      : "bg-[var(--muted)] text-[var(--muted-foreground)]"
                }`}
              >
                {index + 1}. {item.label}
              </li>
            )
          })}
        </ol>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Produto, formato e origem. O rascunho escreve sozinho. A revisão acontece na lista abaixo. O currículo do produto só recebe versões aprovadas.
        </p>
      </div>

      {step === "product" && (
        <div className="space-y-3">
          {products.length === 0 ? (
            <p className="m-0 text-sm">
              Crie um produto em <Link to="/produtos">Produtos</Link> antes de escrever.
            </p>
          ) : (
            <div className="grid gap-2 md:grid-cols-2">
              {products.map((product) => (
                <button
                  key={product.id}
                  type="button"
                  className={`rounded-[var(--radius-md)] border px-3 py-3 text-left text-sm ${
                    productId === product.id ? "border-[var(--primary)] bg-[var(--accent)]" : "border-[var(--border)]"
                  }`}
                  onClick={() => {
                    setProductId(product.id)
                    setStep("format")
                  }}
                >
                  <span className="font-semibold">{product.name}</span>
                  <span className="mt-1 block text-xs text-[var(--muted-foreground)]">
                    {product.description ?? "Sem descrição"}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>
      )}

      {step === "format" && (
        <div className="space-y-3">
          <div className="grid gap-2 md:grid-cols-2">
            {contentFormats.map((item) => (
              <button
                key={item.code}
                type="button"
                className={`rounded-[var(--radius-md)] border px-3 py-3 text-left text-sm ${
                  format === item.code ? "border-[var(--primary)] bg-[var(--accent)]" : "border-[var(--border)]"
                }`}
                onClick={() => {
                  setFormat(item.code)
                  setStep("origin")
                }}
              >
                <span className="font-semibold">{item.label}</span>
                <span className="mt-1 block text-xs text-[var(--muted-foreground)]">{item.detail}</span>
              </button>
            ))}
          </div>
          <Button type="button" variant="ghost" onClick={() => setStep("product")}>
            Trocar produto
          </Button>
        </div>
      )}

      {step === "origin" && (
        <div className="flex flex-wrap gap-2">
          <Button
            type="button"
            variant={origin === "materials" ? "default" : "outline"}
            onClick={() => {
              setOrigin("materials")
              setStep("materials")
            }}
          >
            Meus materiais
          </Button>
          <Button
            type="button"
            variant={origin === "topics" ? "default" : "outline"}
            onClick={() => {
              setOrigin("topics")
              setStep("topics")
            }}
          >
            Temas da área
          </Button>
          <Button type="button" variant="ghost" onClick={() => setStep("format")}>
            Voltar ao formato
          </Button>
        </div>
      )}

      {step === "materials" && (
        <form className="grid gap-3" onSubmit={(event) => void handleMaterials(event)}>
          <div className="flex gap-2">
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
                  materialMode === value
                    ? "bg-[var(--primary)] text-white"
                    : "bg-[var(--background)] text-[var(--muted-foreground)]"
                }`}
                onClick={() => setMaterialMode(value)}
              >
                {label}
              </button>
            ))}
          </div>
          <div className="space-y-2">
            <Label htmlFor="content-title">Título</Label>
            <Input id="content-title" value={title} onChange={(event) => setTitle(event.target.value)} required />
          </div>
          <div className="space-y-2">
            <Label htmlFor="material-name">Nome do material</Label>
            <Input
              id="material-name"
              value={displayName}
              onChange={(event) => setDisplayName(event.target.value)}
              placeholder="Como este material aparece na fonte"
            />
          </div>
          {materialMode === "web" && (
            <div className="space-y-2">
              <Label htmlFor="material-url">URL</Label>
              <Input
                id="material-url"
                value={location}
                onChange={(event) => setLocation(event.target.value)}
                required
                placeholder="https://"
              />
            </div>
          )}
          {materialMode === "file" && (
            <div className="space-y-2">
              <Label htmlFor="material-file">Arquivo</Label>
              <Input
                id="material-file"
                type="file"
                accept=".txt,.md,.pdf,text/plain,text/markdown,application/pdf"
                required
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              />
            </div>
          )}
          {materialMode === "text" && (
            <div className="space-y-2">
              <Label htmlFor="material-text">Texto</Label>
              <textarea
                id="material-text"
                required
                value={text}
                onChange={(event) => setText(event.target.value)}
                className="min-h-28 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-sm"
              />
            </div>
          )}
          <div className="flex gap-2">
            <Button type="submit" disabled={busy || (materialMode === "file" && !file)}>
              {busy ? "Escrevendo…" : "Gerar rascunho"}
            </Button>
            <Button type="button" variant="ghost" onClick={() => setStep("origin")}>
              Voltar
            </Button>
          </div>
        </form>
      )}

      {step === "topics" && (
        <form className="grid gap-3" onSubmit={(event) => void handleSearchTopics(event)}>
          <div className="space-y-2">
            <Label htmlFor="field">Área</Label>
            <select
              id="field"
              required
              className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
              value={fieldId}
              onChange={(event) => setFieldId(event.target.value)}
            >
              <option value="">Selecione</option>
              {fields.map((field) => (
                <option key={field.id} value={field.id}>
                  {field.name}
                </option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label htmlFor="subfield">Subárea (opcional)</Label>
            <select
              id="subfield"
              className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
              value={subfieldId}
              onChange={(event) => setSubfieldId(event.target.value)}
            >
              <option value="">Toda a área</option>
              {subfields.map((field) => (
                <option key={field.id} value={field.id}>
                  {field.name}
                </option>
              ))}
            </select>
          </div>
          <div className="space-y-2">
            <Label htmlFor="window">Janela</Label>
            <select
              id="window"
              className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
              value={windowDays}
              onChange={(event) => setWindowDays(Number(event.target.value))}
            >
              <option value={7}>7 dias</option>
              <option value={30}>30 dias</option>
              <option value={90}>90 dias</option>
            </select>
          </div>
          {statusNote && <p className="m-0 text-xs text-[var(--muted-foreground)]">{statusNote}</p>}
          <div className="flex gap-2">
            <Button type="submit" disabled={busy || !fieldId}>
              {busy ? "Coletando temas…" : "Buscar temas"}
            </Button>
            <Button type="button" variant="ghost" onClick={() => setStep("origin")}>
              Voltar
            </Button>
          </div>
        </form>
      )}

      {step === "choose" && (
        <div className="space-y-3">
          <p className="m-0 text-sm font-medium">Escolha os temas</p>
          <TopicChoiceList
            proposals={proposals}
            selected={selected}
            confirming={busy}
            onToggle={(id) =>
              setSelected((current) =>
                current.includes(id) ? current.filter((item) => item !== id) : [...current, id],
              )
            }
            onConfirm={() => void handleSelect()}
          />
          <Button type="button" variant="ghost" onClick={() => setStep("topics")}>
            Voltar
          </Button>
        </div>
      )}
    </div>
  )
}
