import { Card, CardContent } from "@/components/ui/card"

const metrics = [
  {
    label: "Claims aguardando revisão",
    value: "12",
    hint: "4 de alta prioridade",
    hintClass: "text-[var(--warning)]",
  },
  {
    label: "Fontes monitoradas",
    value: "24",
    hint: "3 atualizadas hoje",
    hintClass: "text-[var(--info)]",
  },
  {
    label: "Conteúdos aprovados",
    value: "18",
    hint: "+4 nesta semana",
    hintClass: "text-[var(--success)]",
  },
  {
    label: "Taxa de resultado",
    value: "78%",
    hint: "+6 p.p. na edição atual",
    hintClass: "text-[var(--success)]",
  },
]

const priorityWork = [
  {
    n: "1",
    title: "Revisar 3 claims afetadas",
    subtitle: "Strands Agents v1.8",
    tag: "Conhecimento",
    tone: "bg-[var(--warning)]",
  },
  {
    n: "2",
    title: "Aprovar Aula 4.2",
    subtitle: "Curso AI Agents",
    tag: "Conteúdo",
    tone: "bg-[var(--primary)]",
  },
  {
    n: "3",
    title: "Exportar edição 2026.09",
    subtitle: "Wolverine em Produção",
    tag: "Publicação",
    tone: "bg-[var(--success)]",
  },
]

const pipeline = [
  { label: "Pesquisa", status: "7 em execução", width: "70%", color: "bg-[var(--info)]" },
  { label: "Revisão", status: "12 pendentes", width: "55%", color: "bg-[var(--warning)]" },
  { label: "Conteúdo", status: "4 gerando", width: "35%", color: "bg-[var(--primary)]" },
  { label: "Publicação", status: "2 prontos", width: "20%", color: "bg-[var(--success)]" },
]

const activity = [
  { title: "Claim aprovada", detail: "por Paulo Roberto", when: "há 18 min", dot: "bg-[var(--success)]" },
  { title: "Snapshot atualizado", detail: "Strands Agents docs", when: "há 2 h", dot: "bg-[var(--info)]" },
  { title: "Pesquisa concluída", detail: "Wolverine durability", when: "há 3 h", dot: "bg-[var(--primary)]" },
]

export function DashboardPage() {
  return (
    <div className="space-y-6">
      <section>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
          Bom dia, Paulo
        </h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Aqui está o que precisa da sua atenção hoje.
        </p>
      </section>

      <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        {metrics.map((metric) => (
          <Card key={metric.label}>
            <CardContent className="p-5">
              <div className="text-xs font-medium text-[var(--muted-foreground)]">
                {metric.label}
              </div>
              <div className="mt-2 text-[28px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
                {metric.value}
              </div>
              <div className={`mt-3 text-xs font-medium ${metric.hintClass}`}>{metric.hint}</div>
            </CardContent>
          </Card>
        ))}
      </section>

      <section className="grid gap-4 xl:grid-cols-[1.35fr_0.65fr]">
        <Card>
          <CardContent className="p-6">
            <h3 className="m-0 text-lg font-semibold text-[var(--foreground)]">
              Trabalho prioritário
            </h3>
            <ul className="mt-5 space-y-4">
              {priorityWork.map((item) => (
                <li
                  key={item.n}
                  className="flex items-start justify-between gap-4 rounded-[var(--radius-md)] border border-[var(--border)] px-4 py-3"
                >
                  <div className="flex items-start gap-3">
                    <span
                      className={`grid size-7 shrink-0 place-items-center rounded-full text-[13px] font-semibold text-white ${item.tone}`}
                    >
                      {item.n}
                    </span>
                    <div>
                      <p className="m-0 text-sm font-semibold text-[var(--foreground)]">
                        {item.title}
                      </p>
                      <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
                        {item.subtitle}
                      </p>
                    </div>
                  </div>
                  <span className="shrink-0 text-xs text-[var(--muted-foreground)]">{item.tag}</span>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-6">
            <h3 className="m-0 text-lg font-semibold text-[var(--foreground)]">Pipeline</h3>
            <ul className="mt-5 space-y-5">
              {pipeline.map((stage) => (
                <li key={stage.label}>
                  <div className="mb-2 flex items-center justify-between gap-3 text-xs">
                    <span className="text-[var(--muted-foreground)]">{stage.label}</span>
                    <span className="font-medium text-[var(--foreground)]">{stage.status}</span>
                  </div>
                  <div className="h-1.5 overflow-hidden rounded-full bg-[var(--background)]">
                    <div className={`h-full rounded-full ${stage.color}`} style={{ width: stage.width }} />
                  </div>
                </li>
              ))}
            </ul>
          </CardContent>
        </Card>
      </section>

      <Card>
        <CardContent className="p-6">
          <h3 className="m-0 text-lg font-semibold text-[var(--foreground)]">Atividade recente</h3>
          <ul className="mt-4 divide-y divide-[var(--border)]">
            {activity.map((item) => (
              <li key={item.title} className="flex items-center justify-between gap-4 py-3.5">
                <div className="flex min-w-0 items-center gap-3">
                  <span className={`size-2 shrink-0 rounded-full ${item.dot}`} />
                  <span className="text-[13px] font-medium text-[var(--foreground)]">
                    {item.title}
                  </span>
                  <span className="truncate text-xs text-[var(--muted-foreground)]">
                    {item.detail}
                  </span>
                </div>
                <span className="shrink-0 text-xs text-[var(--muted-foreground)]">{item.when}</span>
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>
    </div>
  )
}
