import { FormEvent, useEffect, useState } from "react"
import { toast } from "sonner"

import { TopicChoiceList } from "@/components/topic-choice-list"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import {
  collectEditorialSeries,
  createEditorialSeries,
  deleteEditorialSeries,
  getTopicDiscovery,
  listEditorialSeries,
  listTopicAreas,
  listTopicDiscoveries,
  selectDiscoveredTopics,
  type EditorialSeries,
  type TopicArea,
  type TopicDiscovery,
} from "@/lib/api/editorial"
import { listProducts, type ProductListItem } from "@/lib/api/catalog"
import { watchOperation } from "@/lib/api/operations"
import { contentFormats, taskStatus } from "@/lib/task-status"

export function SeriesPanel({ onCreated }: { onCreated: () => Promise<void> }) {
  const [series, setSeries] = useState<EditorialSeries[]>([])
  const [fresh, setFresh] = useState<TopicDiscovery[]>([])
  const [fields, setFields] = useState<TopicArea[]>([])
  const [products, setProducts] = useState<ProductListItem[]>([])
  const [productId, setProductId] = useState("")
  const [format, setFormat] = useState("newsletter")
  const [fieldId, setFieldId] = useState("")
  const [windowDays, setWindowDays] = useState(30)
  const [cadence, setCadence] = useState("weekly")
  const [active, setActive] = useState<TopicDiscovery | null>(null)
  const [selected, setSelected] = useState<string[]>([])
  const [busy, setBusy] = useState(false)

  async function reload() {
    const [seriesPage, discoveries, areas, productPage] = await Promise.all([
      listEditorialSeries(),
      listTopicDiscoveries("AwaitingSelection"),
      listTopicAreas(),
      listProducts(),
    ])
    setSeries(seriesPage)
    setFresh(discoveries)
    setFields(areas)
    setProducts(productPage.items)
    setProductId((current) => current || productPage.items[0]?.id || "")
  }

  useEffect(() => {
    void reload().catch(() => undefined)
  }, [])

  async function handleCreate(event: FormEvent) {
    event.preventDefault()
    const area = fields.find((item) => item.id === fieldId)
    if (!area) {
      return
    }
    setBusy(true)
    try {
      await createEditorialSeries({
        format,
        areaId: area.id,
        areaName: area.name,
        areaIsSubfield: false,
        windowDays,
        cadence,
      })
      toast.success("Série criada. A cadência só propõe temas.")
      await reload()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível criar a série.")
    } finally {
      setBusy(false)
    }
  }

  async function handleCollect(seriesId: string) {
    setBusy(true)
    try {
      const started = await collectEditorialSeries(seriesId)
      let discovery = await getTopicDiscovery(started.discoveryId)
      if (discovery.operationId && discovery.status === "Collecting") {
        await watchOperation(discovery.operationId, () => undefined)
        discovery = await getTopicDiscovery(started.discoveryId)
      }
      setActive(discovery)
      setSelected([])
      await reload()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível coletar temas.")
    } finally {
      setBusy(false)
    }
  }

  async function handleSelect() {
    if (!active || !productId) {
      toast.error("Escolha o produto antes de escrever.")
      return
    }
    setBusy(true)
    try {
      await selectDiscoveredTopics(active.id, selected, productId)
      toast.success("Escrevendo o rascunho")
      setActive(null)
      await onCreated()
      await reload()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível escrever o rascunho.")
    } finally {
      setBusy(false)
    }
  }

  async function handleDelete(seriesId: string) {
    if (!window.confirm("Apagar esta série? A cadência para de propor temas.")) {
      return
    }
    try {
      await deleteEditorialSeries(seriesId)
      toast.success("Série apagada.")
      await reload()
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Não foi possível apagar a série.")
    }
  }

  return (
    <div className="space-y-4">
      <div>
        <h2 className="m-0 text-lg font-semibold">Séries</h2>
        <p className="mb-0 mt-1 text-sm text-[var(--muted-foreground)]">
          A cadência cria temas novos e para. Ela não escreve nem publica sozinha.
        </p>
      </div>
      <form className="grid gap-3 md:grid-cols-2" onSubmit={(event) => void handleCreate(event)}>
        <div className="space-y-2">
          <Label htmlFor="series-product">Produto dos rascunhos</Label>
          <select
            id="series-product"
            required
            className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
            value={productId}
            onChange={(event) => setProductId(event.target.value)}
          >
            <option value="">Selecione</option>
            {products.map((product) => (
              <option key={product.id} value={product.id}>
                {product.name}
              </option>
            ))}
          </select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="series-format">Formato</Label>
          <select
            id="series-format"
            className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
            value={format}
            onChange={(event) => setFormat(event.target.value)}
          >
            {contentFormats.map((item) => (
              <option key={item.code} value={item.code}>
                {item.label}
              </option>
            ))}
          </select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="series-area">Área</Label>
          <select
            id="series-area"
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
          <Label htmlFor="series-window">Janela</Label>
          <select
            id="series-window"
            className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
            value={windowDays}
            onChange={(event) => setWindowDays(Number(event.target.value))}
          >
            <option value={7}>7 dias</option>
            <option value={30}>30 dias</option>
            <option value={90}>90 dias</option>
          </select>
        </div>
        <div className="space-y-2">
          <Label htmlFor="series-cadence">Cadência</Label>
          <select
            id="series-cadence"
            className="flex h-9 w-full rounded-[var(--radius-md)] border border-[var(--border)] bg-transparent px-3 text-sm"
            value={cadence}
            onChange={(event) => setCadence(event.target.value)}
          >
            <option value="manual">Manual</option>
            <option value="weekly">Semanal</option>
            <option value="monthly">Mensal</option>
          </select>
        </div>
        <div className="md:col-span-2">
          <Button type="submit" disabled={busy}>
            Criar série
          </Button>
        </div>
      </form>

      <div className="space-y-2">
        <h3 className="m-0 text-sm font-semibold">Temas novos</h3>
        {fresh.length === 0 && (
          <p className="m-0 text-sm text-[var(--muted-foreground)]">Nenhum tema esperando escolha.</p>
        )}
        {fresh.map((discovery) => (
          <button
            key={discovery.id}
            type="button"
            className="block w-full rounded-[var(--radius-md)] border border-[var(--border)] px-3 py-2 text-left text-sm"
            onClick={() => {
              setActive(discovery)
              setSelected([])
            }}
          >
            {discovery.areaName} · {taskStatus(discovery.status)} · {discovery.proposals.length} temas
          </button>
        ))}
      </div>

      {active && (
        <TopicChoiceList
          proposals={active.proposals}
          selected={selected}
          confirming={busy}
          onToggle={(id) =>
            setSelected((current) =>
              current.includes(id) ? current.filter((item) => item !== id) : [...current, id],
            )
          }
          onConfirm={() => void handleSelect()}
        />
      )}

      <ul className="space-y-2 pl-0">
        {series.map((item) => (
          <li
            key={item.id}
            className="flex flex-wrap items-center justify-between gap-2 rounded-[var(--radius-md)] border border-[var(--border)] px-3 py-2 text-sm"
          >
            <span>
              {item.areaName} · {item.cadence === "Weekly" ? "semanal" : item.cadence === "Monthly" ? "mensal" : "manual"}
            </span>
            <Button type="button" variant="outline" disabled={busy} onClick={() => void handleCollect(item.id)}>
              Coletar temas
            </Button>
            <Button type="button" variant="ghost" onClick={() => void handleDelete(item.id)}>
              Excluir
            </Button>
          </li>
        ))}
      </ul>
    </div>
  )
}
