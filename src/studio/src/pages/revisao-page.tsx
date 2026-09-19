import { useSearchParams } from "react-router"

import { ContentReviewPanel } from "@/components/content-review-panel"
import { ClaimsPage } from "@/pages/claims-page"
import { cn } from "@/lib/utils"

export function RevisaoPage() {
  const [params, setParams] = useSearchParams()
  const tab = params.get("aba") === "conteudo" ? "conteudo" : "afirmacoes"

  function select(next: "afirmacoes" | "conteudo") {
    setParams(next === "afirmacoes" ? {} : { aba: "conteudo" }, { replace: true })
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="m-0 text-xs font-semibold uppercase tracking-[0.12em] text-[var(--primary)]">
            Revisão
          </p>
          <h1 className="m-0 mt-1 text-[30px] font-bold tracking-[-0.03em]">Uma decisão por vez</h1>
          <p className="mb-0 mt-2 max-w-2xl text-sm text-[var(--muted-foreground)]">
            A IA propõe. Aprovar ou rejeitar exige a sua intenção. Nada daqui publica.
          </p>
        </div>
        <div className="flex gap-2" role="tablist" aria-label="Tipo de revisão">
          <TabButton active={tab === "afirmacoes"} onClick={() => select("afirmacoes")}>
            Afirmações
          </TabButton>
          <TabButton active={tab === "conteudo"} onClick={() => select("conteudo")}>
            Conteúdo
          </TabButton>
        </div>
      </div>
      {tab === "afirmacoes" ? <ClaimsPage embedded /> : <ContentReviewPanel />}
    </div>
  )
}

function TabButton({
  active,
  onClick,
  children,
}: {
  active: boolean
  onClick: () => void
  children: string
}) {
  return (
    <button
      type="button"
      role="tab"
      aria-selected={active}
      className={cn(
        "rounded-full px-3 py-1.5 text-xs font-semibold",
        active
          ? "bg-[var(--foreground)] text-white"
          : "border border-[var(--border)] bg-[var(--card)] text-[var(--muted-foreground)]",
      )}
      onClick={onClick}
    >
      {children}
    </button>
  )
}
