import { Plus } from "lucide-react"
import { Link } from "react-router"

import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"

const statusTabs = [
  { label: "Todas", count: 0, active: true },
  { label: "Aguardando", count: 0, active: false },
  { label: "Aprovadas", count: 0, active: false },
  { label: "Com falha", count: 0, active: false, danger: true },
]

export function SourcesPage() {
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
        <Button>
          <Plus className="size-4" />
          Adicionar fonte
        </Button>
      </div>

      <div className="flex flex-wrap gap-2">
        {statusTabs.map((tab) => (
          <button
            key={tab.label}
            type="button"
            className={`rounded-full px-3.5 py-1.5 text-xs font-semibold ${
              tab.active
                ? "bg-[var(--accent)] text-[var(--primary)]"
                : tab.danger
                  ? "bg-[var(--background)] text-[var(--destructive)]"
                  : "bg-[var(--background)] text-[var(--muted-foreground)]"
            }`}
          >
            {tab.label} {tab.count}
          </button>
        ))}
      </div>

      <div className="flex flex-wrap gap-3">
        <Input className="max-w-sm" placeholder="Buscar por título ou URL" />
        <button
          type="button"
          className="h-12 rounded-[10px] border border-[var(--border)] bg-[var(--card)] px-3 text-[13px] text-[var(--muted-foreground)]"
        >
          Tipo: todos
        </button>
        <button
          type="button"
          className="h-12 rounded-[10px] border border-[var(--border)] bg-[var(--card)] px-3 text-[13px] text-[var(--muted-foreground)]"
        >
          Integridade: todos
        </button>
      </div>

      <Card>
        <CardContent className="overflow-x-auto p-0">
          <table className="w-full min-w-[720px] border-collapse text-left text-sm">
            <thead>
              <tr className="border-b border-[var(--border)] text-xs text-[var(--muted-foreground)]">
                <th className="px-5 py-3 font-semibold">Fonte</th>
                <th className="px-5 py-3 font-semibold">Tipo</th>
                <th className="px-5 py-3 font-semibold">Integridade</th>
                <th className="px-5 py-3 font-semibold">Atualização</th>
                <th className="px-5 py-3 font-semibold">Status</th>
                <th className="px-5 py-3 font-semibold" />
              </tr>
            </thead>
            <tbody>
              <tr>
                <td colSpan={6} className="px-5 py-16 text-center text-sm text-[var(--muted-foreground)]">
                  Nenhuma fonte cadastrada ainda. Adicione a primeira origem para iniciar o
                  monitoramento.
                </td>
              </tr>
            </tbody>
          </table>
          <div className="flex items-center justify-between border-t border-[var(--border)] px-5 py-3 text-xs text-[var(--muted-foreground)]">
            <span>0–0 de 0</span>
            <span className="tracking-widest">‹ 1 ›</span>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
