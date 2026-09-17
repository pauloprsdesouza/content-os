import { LoadingState } from "@/components/page-states"

export function PlaceholderPage({ name }: { name: string }) {
  return (
    <div className="space-y-7">
      <div>
        <p className="mb-1 text-sm text-[var(--muted)]">Módulo em preparação</p>
        <h2 className="m-0 text-3xl font-semibold tracking-[-0.04em]">{name}</h2>
      </div>
      <LoadingState label={`Preparando ${name.toLocaleLowerCase("pt-BR")}…`} />
    </div>
  )
}
