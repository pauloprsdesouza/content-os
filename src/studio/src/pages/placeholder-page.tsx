import { LoadingState } from "@/components/page-states"

export function PlaceholderPage({ name }: { name: string }) {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
          {name}
        </h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Módulo em preparação — layout alinhado ao shell do Content Studio.
        </p>
      </div>
      <LoadingState label={`Preparando ${name.toLocaleLowerCase("pt-BR")}…`} />
    </div>
  )
}
