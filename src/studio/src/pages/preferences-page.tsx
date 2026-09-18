import { Card, CardContent } from "@/components/ui/card"

export function PreferencesPage() {
  return (
    <div className="max-w-xl space-y-6">
      <section>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em]">Preferências</h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          O Studio desta versão é fixo em pt-BR e no tema claro do Figma.
        </p>
      </section>
      <Card>
        <CardContent className="space-y-3 p-6 text-sm">
          <p className="m-0">Idioma: português (Brasil)</p>
          <p className="m-0">Tema: claro</p>
          <p className="m-0 text-xs text-[var(--muted-foreground)]">
            Não há endpoint para gravar preferências.
          </p>
        </CardContent>
      </Card>
    </div>
  )
}
