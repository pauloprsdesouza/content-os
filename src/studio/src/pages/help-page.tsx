import { Card, CardContent } from "@/components/ui/card"

export function HelpPage() {
  return (
    <div className="max-w-xl space-y-6">
      <section>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em]">Ajuda e suporte</h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Decisões de claim, conteúdo e publicação ficam nas filas do Studio. Não há canal de
          suporte nesta API.
        </p>
      </section>
      <Card>
        <CardContent className="space-y-2 p-6 text-sm">
          <p className="m-0">Afirmações para revisar: atalhos A e R na fila.</p>
          <p className="m-0">Vendas: um pedido só confirma se a Kiwify devolver a compra paga.</p>
        </CardContent>
      </Card>
    </div>
  )
}
