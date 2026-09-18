import { Button } from "@/components/ui/button"
import type { TopicProposal } from "@/lib/api/editorial"

export function TopicChoiceList({
  proposals,
  selected,
  onToggle,
  onConfirm,
  confirming,
}: {
  proposals: TopicProposal[]
  selected: string[]
  onToggle: (id: string) => void
  onConfirm: () => void
  confirming: boolean
}) {
  if (proposals.length === 0) {
    return (
      <p className="m-0 text-sm text-[var(--muted-foreground)]">
        Nenhum tema nesse recorte. A busca vazia permanece vazia.
      </p>
    )
  }

  return (
    <div className="space-y-3">
      {proposals.map((proposal) => (
        <label
          key={proposal.id}
          className="flex cursor-pointer gap-3 rounded-[var(--radius-md)] border border-[var(--border)] px-3 py-3 text-sm"
        >
          <input
            type="checkbox"
            checked={selected.includes(proposal.id)}
            onChange={() => onToggle(proposal.id)}
          />
          <span>
            <span className="font-medium">{proposal.label}</span>
            {proposal.rationale && (
              <span className="mt-1 block text-xs text-[var(--muted-foreground)]">{proposal.rationale}</span>
            )}
            <span className="mt-1 block text-xs text-[var(--muted-foreground)]">
              {proposal.workIds.length} {proposal.workIds.length === 1 ? "obra" : "obras"}
            </span>
          </span>
        </label>
      ))}
      <Button type="button" disabled={confirming || selected.length === 0} onClick={onConfirm}>
        {confirming ? "Escrevendo…" : "Escrever rascunho"}
      </Button>
    </div>
  )
}
