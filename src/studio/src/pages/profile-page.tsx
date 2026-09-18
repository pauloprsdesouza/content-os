import { Card, CardContent } from "@/components/ui/card"
import { useAuth } from "@/lib/auth"

export function ProfilePage() {
  const { session } = useAuth()
  const email = session?.userName ?? "—"

  return (
    <div className="mx-auto max-w-xl space-y-6">
      <section>
        <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em]">Perfil</h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          Somente leitura. Troca de senha e MFA não existem nesta API.
        </p>
      </section>
      <Card>
        <CardContent className="space-y-4 p-6">
          <div>
            <p className="m-0 text-xs font-medium uppercase tracking-wide text-[var(--muted-foreground)]">
              E-mail
            </p>
            <p className="mb-0 mt-1 text-sm font-medium">{email}</p>
          </div>
          <div>
            <p className="m-0 text-xs font-medium uppercase tracking-wide text-[var(--muted-foreground)]">
              Sessão
            </p>
            <p className="mb-0 mt-1 text-sm">Cookie HttpOnly. Encerre em Sair.</p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
