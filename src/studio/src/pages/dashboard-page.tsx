import { ArrowUpRight, BookOpenCheck, CircleCheck, FileText, Library } from "lucide-react"

import { EmptyState } from "@/components/page-states"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"

const metrics = [
  { label: "Fontes ativas", value: "—", icon: Library },
  { label: "Claims em revisão", value: "—", icon: BookOpenCheck },
  { label: "Conteúdos em produção", value: "—", icon: FileText },
  { label: "Publicações confirmadas", value: "—", icon: CircleCheck },
]

export function DashboardPage() {
  return (
    <div className="space-y-8">
      <section className="flex flex-col justify-between gap-5 md:flex-row md:items-end">
        <div>
          <p className="mb-2 text-xs font-bold uppercase tracking-[0.18em] text-[var(--accent-strong)]">
            Controle editorial
          </p>
          <h2 className="brand-wordmark m-0 max-w-3xl text-4xl leading-[1.08] tracking-[-0.04em] md:text-5xl">
            Da fonte ao resultado, com cada decisão rastreável.
          </h2>
        </div>
        <p className="m-0 max-w-sm text-sm leading-relaxed text-[var(--muted)]">
          Acompanhe pesquisa, revisão humana e publicação em um fluxo operacional único.
        </p>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {metrics.map(({ label, value, icon: Icon }) => (
          <Card key={label}>
            <CardContent className="p-5">
              <div className="mb-7 flex items-center justify-between">
                <div className="grid size-9 place-items-center rounded-lg bg-[var(--ink)] text-[var(--accent)]">
                  <Icon className="size-4" />
                </div>
                <ArrowUpRight className="size-4 text-[var(--muted-light)]" />
              </div>
              <div className="text-3xl font-semibold tracking-[-0.04em]">{value}</div>
              <div className="mt-1 text-xs font-medium text-[var(--muted)]">{label}</div>
            </CardContent>
          </Card>
        ))}
      </section>

      <section className="grid gap-5 xl:grid-cols-[1.4fr_0.6fr]">
        <EmptyState
          title="Sua operação começa aqui"
          description="Conecte a primeira fonte para iniciar o fluxo de pesquisa e construir uma base de claims verificáveis."
        />
        <Card>
          <CardHeader>
            <CardTitle>Atividade recente</CardTitle>
            <CardDescription>Eventos relevantes da operação.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="border-l border-dashed border-[var(--line)] py-4 pl-5 text-sm text-[var(--muted)]">
              Nenhuma atividade registrada ainda.
            </div>
          </CardContent>
        </Card>
      </section>
    </div>
  )
}
