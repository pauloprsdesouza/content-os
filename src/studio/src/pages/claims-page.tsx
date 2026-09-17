import { EmptyState } from "@/components/page-states"
import { Card, CardContent } from "@/components/ui/card"

export function ClaimsPage() {
  return (
    <div className="space-y-7">
      <div>
        <p className="mb-1 text-sm text-[var(--muted)]">Gate de qualidade</p>
        <h2 className="m-0 text-3xl font-semibold tracking-[-0.04em]">Revisão de claims</h2>
      </div>
      <Card>
        <CardContent className="flex flex-wrap gap-2 p-4">
          {["Pendentes 0", "Aprovados 0", "Rejeitados 0"].map((filter, index) => (
            <button
              key={filter}
              className={`rounded-full px-3 py-1.5 text-xs font-semibold ${
                index === 0
                  ? "bg-[var(--ink)] text-white"
                  : "bg-[var(--canvas)] text-[var(--muted)]"
              }`}
            >
              {filter}
            </button>
          ))}
        </CardContent>
      </Card>
      <EmptyState
        title="Fila de revisão vazia"
        description="Claims extraídos durante a pesquisa aparecerão aqui para validação humana antes de entrarem na base de conhecimento."
      />
    </div>
  )
}
