import { Inbox, LoaderCircle } from "lucide-react"

import { Card } from "@/components/ui/card"

export function LoadingState({ label = "Carregando dados…" }: { label?: string }) {
  return (
    <Card className="flex min-h-44 items-center justify-center gap-3 text-sm text-[var(--muted)]">
      <LoaderCircle className="size-5 animate-spin text-[var(--accent-strong)]" />
      {label}
    </Card>
  )
}

export function EmptyState({
  title,
  description,
}: {
  title: string
  description: string
}) {
  return (
    <Card className="dot-grid flex min-h-52 flex-col items-center justify-center px-6 text-center">
      <div className="mb-4 grid size-11 place-items-center rounded-full bg-[var(--accent)]/15 text-[var(--accent-strong)]">
        <Inbox className="size-5" />
      </div>
      <h2 className="m-0 text-base font-semibold">{title}</h2>
      <p className="mb-0 mt-2 max-w-md text-sm leading-relaxed text-[var(--muted)]">
        {description}
      </p>
    </Card>
  )
}
