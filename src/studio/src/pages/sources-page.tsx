import { Plus } from "lucide-react"

import { EmptyState } from "@/components/page-states"
import { Button } from "@/components/ui/button"

export function SourcesPage() {
  return (
    <div className="space-y-7">
      <div className="flex items-end justify-between gap-4">
        <div>
          <p className="mb-1 text-sm text-[var(--muted)]">Base de conhecimento</p>
          <h2 className="m-0 text-3xl font-semibold tracking-[-0.04em]">Fontes confiáveis</h2>
        </div>
        <Button variant="accent">
          <Plus className="size-4" />
          Nova fonte
        </Button>
      </div>
      <EmptyState
        title="Nenhuma fonte cadastrada"
        description="Adicione documentos ou endereços aprovados para que a pesquisa tenha uma origem clara e auditável."
      />
    </div>
  )
}
