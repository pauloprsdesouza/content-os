import { Button } from "@/components/ui/button"
import { Card, CardContent } from "@/components/ui/card"

export function ClaimsPage() {
  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
            Revisão de evidências
          </h2>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Claim — de — · Atalhos: A aprova · R rejeita · S ignora
          </p>
        </div>
        <Button variant="outline">Salvar e sair</Button>
      </div>

      <div className="grid gap-4 xl:grid-cols-[1.35fr_0.65fr]">
        <Card>
          <CardContent className="space-y-6 p-6">
            <div>
              <p className="m-0 text-[11px] font-bold uppercase tracking-[0.12em] text-[var(--primary)]">
                Claim proposta
              </p>
              <p className="mb-0 mt-3 text-2xl font-bold leading-snug tracking-[-0.02em] text-[var(--foreground)]">
                Nenhuma claim na fila de revisão.
              </p>
            </div>

            <div>
              <p className="m-0 text-[11px] font-bold uppercase tracking-[0.12em] text-[var(--muted-foreground)]">
                Evidências relacionadas
              </p>
              <div className="mt-4 rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-4 py-8 text-center text-sm text-[var(--muted-foreground)]">
                Evidências aparecerão aqui quando houver claims extraídas da pesquisa.
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="space-y-5 p-6">
            <h3 className="m-0 text-lg font-semibold text-[var(--foreground)]">Decisão</h3>

            <div>
              <p className="mb-2 text-xs font-semibold text-[var(--muted-foreground)]">
                Força do suporte
              </p>
              <div className="flex flex-wrap gap-2">
                {["Forte", "Médio", "Fraco"].map((option, index) => (
                  <button
                    key={option}
                    type="button"
                    disabled
                    className={`rounded-full px-3 py-1.5 text-xs font-semibold ${
                      index === 0
                        ? "bg-[color-mix(in_srgb,var(--success)_18%,white)] text-[var(--success)]"
                        : "bg-[var(--background)] text-[var(--muted-foreground)]"
                    }`}
                  >
                    {option}
                  </button>
                ))}
              </div>
            </div>

            <div>
              <p className="mb-2 text-xs font-semibold text-[var(--muted-foreground)]">
                Notas do revisor
              </p>
              <textarea
                disabled
                className="min-h-24 w-full resize-none rounded-[var(--radius-md)] border border-[var(--border)] bg-[var(--background)] px-3 py-2 text-[13px] text-[var(--muted-foreground)]"
                placeholder="Registre a decisão quando houver claims na fila."
              />
            </div>

            <div className="rounded-[var(--radius-md)] bg-[var(--accent)] px-3 py-3 text-xs text-[var(--foreground)]">
              <p className="m-0 font-semibold text-[var(--primary)]">✦ Sugestão da IA</p>
              <p className="mb-0 mt-1 text-[var(--muted-foreground)]">
                Disponíveis após a primeira extração de claims.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <Button variant="destructive" disabled>
                Rejeitar
              </Button>
              <Button disabled>Aprovar</Button>
            </div>

            <div className="flex items-center justify-between text-xs text-[var(--muted-foreground)]">
              <span>← Claim anterior</span>
              <span>Próxima claim →</span>
            </div>
            <div>
              <div className="mb-1 h-1.5 overflow-hidden rounded-full bg-[var(--background)]">
                <div className="h-full w-0 rounded-full bg-[var(--primary)]" />
              </div>
              <p className="mb-0 text-center text-xs text-[var(--muted-foreground)]">0 de 0</p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}
