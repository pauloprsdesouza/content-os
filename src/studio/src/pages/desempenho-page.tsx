import { useSearchParams } from "react-router"

import { ResultadosPage } from "@/pages/resultados-page"
import { VendasPage } from "@/pages/vendas-page"
import { cn } from "@/lib/utils"

export function DesempenhoPage() {
  const [params, setParams] = useSearchParams()
  const tab = params.get("aba") === "resultados" ? "resultados" : "vendas"

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--primary)]">
            Desempenho
          </p>
          <h1 className="m-0 mt-1 text-[30px] font-bold tracking-[-0.03em]">Leitura, não edição</h1>
          <p className="mb-0 mt-2 max-w-2xl text-sm text-[var(--muted-foreground)]">
            Vendas reconciliadas e resultado do aluno. Isso não compete com a fila de decisão.
          </p>
        </div>
        <div className="flex gap-2" role="tablist" aria-label="Desempenho">
          <button
            type="button"
            role="tab"
            aria-selected={tab === "vendas"}
            className={cn(
              "rounded-full px-3 py-1.5 text-xs font-semibold",
              tab === "vendas"
                ? "bg-[var(--foreground)] text-white"
                : "border border-[var(--border)] bg-[var(--card)] text-[var(--muted-foreground)]",
            )}
            onClick={() => setParams({}, { replace: true })}
          >
            Vendas
          </button>
          <button
            type="button"
            role="tab"
            aria-selected={tab === "resultados"}
            className={cn(
              "rounded-full px-3 py-1.5 text-xs font-semibold",
              tab === "resultados"
                ? "bg-[var(--foreground)] text-white"
                : "border border-[var(--border)] bg-[var(--card)] text-[var(--muted-foreground)]",
            )}
            onClick={() => setParams({ aba: "resultados" }, { replace: true })}
          >
            Resultados
          </button>
        </div>
      </div>
      {tab === "vendas" ? <VendasPage embedded /> : <ResultadosPage embedded />}
    </div>
  )
}
