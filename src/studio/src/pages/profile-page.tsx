import { NavLink, useLocation } from "react-router"

import { Card, CardContent } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useAuth } from "@/lib/auth"
import { cn } from "@/lib/utils"

function initials(userName: string | null | undefined) {
  if (!userName) {
    return "CO"
  }
  const local = userName.split("@")[0] ?? userName
  const parts = local.split(/[.\s_-]+/).filter(Boolean)
  return parts
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("")
}

export function ProfilePage() {
  const { session } = useAuth()
  const location = useLocation()
  const security = location.pathname.endsWith("/seguranca")
  const email = session?.userName ?? "—"
  const displayName = email.includes("@") ? email.split("@")[0] : email

  return (
    <div className="space-y-6">
      <section>
        <p className="m-0 text-xs text-[var(--muted-foreground)]">Conta / Meu perfil</p>
        <h2 className="m-0 mt-2 text-[30px] font-bold tracking-[-0.03em]">
          {security ? "Segurança e sessões" : "Meu perfil"}
        </h2>
        <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
          {security
            ? "Proteja o acesso e revise onde sua conta está conectada."
            : "Gerencie sua identidade e como ela aparece nas aprovações."}
        </p>
      </section>

      <div className="flex gap-6 border-b border-[var(--border)] text-sm">
        <NavLink
          className={({ isActive }) =>
            cn(
              "border-b-2 pb-3 font-medium",
              isActive && !security
                ? "border-[var(--primary)] text-[var(--foreground)]"
                : "border-transparent text-[var(--muted-foreground)]",
            )
          }
          end
          to="/perfil"
        >
          Perfil
        </NavLink>
        <NavLink
          className={({ isActive }) =>
            cn(
              "border-b-2 pb-3 font-medium",
              isActive
                ? "border-[var(--primary)] text-[var(--foreground)]"
                : "border-transparent text-[var(--muted-foreground)]",
            )
          }
          to="/perfil/seguranca"
        >
          Segurança e sessões
        </NavLink>
      </div>

      {security ? <SecurityPanel email={email} /> : <OverviewPanel displayName={displayName} email={email} />}
    </div>
  )
}

function OverviewPanel({ displayName, email }: { displayName: string; email: string }) {
  return (
    <div className="grid gap-4 lg:grid-cols-[320px_1fr]">
      <div className="space-y-4">
        <Card>
          <CardContent className="space-y-3 p-6">
            <div className="grid size-14 place-items-center rounded-full bg-[var(--accent)] text-lg font-semibold text-[var(--foreground)]">
              {initials(email)}
            </div>
            <p className="m-0 text-lg font-semibold">{displayName}</p>
            <p className="m-0 text-xs text-[var(--muted-foreground)]">{email}</p>
            <p className="m-0 border-t border-[var(--border)] pt-3 text-[11px] text-[var(--muted-foreground)]">
              Conta local protegida por sessão autenticada.
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="space-y-2 p-6">
            <p className="m-0 text-sm font-semibold">Status da conta</p>
            <p className="m-0 text-sm">Sessão ativa</p>
            <p className="m-0 text-[11px] text-[var(--muted-foreground)]">Autenticado como {email}</p>
            <p className="m-0 text-[11px] text-[var(--muted-foreground)]">
              A sessão não informa papel. Usuários e papéis ainda não têm API.
            </p>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardContent className="space-y-4 p-6">
          <div>
            <p className="m-0 text-base font-semibold">Informações pessoais</p>
            <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
              Dados usados na autoria, aprovação e auditoria.
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="displayName">Nome de exibição</Label>
            <Input id="displayName" value={displayName} readOnly />
            <p className="m-0 text-[11px] text-[var(--muted-foreground)]">
              A API só devolve o e-mail da sessão. Salvar nome ainda não existe.
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="email">E-mail</Label>
            <Input id="email" value={email} readOnly />
            <p className="m-0 text-[11px] text-[var(--muted-foreground)]">
              O e-mail é gerenciado pela identidade da conta.
            </p>
          </div>
          <div className="flex justify-end">
            <button
              type="button"
              disabled
              className="h-9 rounded-[var(--radius-md)] bg-[var(--primary)] px-4 text-xs font-semibold text-white opacity-45"
            >
              Salvar
            </button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}

function SecurityPanel({ email }: { email: string }) {
  return (
    <Card>
      <CardContent className="max-w-xl space-y-4 p-6">
        <div>
          <p className="m-0 text-base font-semibold">Alterar senha</p>
          <p className="mb-0 mt-1 text-xs text-[var(--muted-foreground)]">
            A API desta versão não troca senha nem lista outras sessões.
          </p>
        </div>
        <div className="space-y-2">
          <Label htmlFor="currentPassword">Senha atual</Label>
          <Input id="currentPassword" type="password" disabled placeholder="••••••••••••" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="nextPassword">Nova senha</Label>
          <Input id="nextPassword" type="password" disabled placeholder="Mínimo de 12 caracteres" />
        </div>
        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Confirmar nova senha</Label>
          <Input id="confirmPassword" type="password" disabled placeholder="Repita a nova senha" />
        </div>
        <button
          type="button"
          disabled
          className="h-9 rounded-[var(--radius-md)] bg-[var(--primary)] px-4 text-xs font-semibold text-white opacity-45"
        >
          Atualizar senha
        </button>
        <p className="m-0 text-[11px] text-[var(--muted-foreground)]">Sessão atual: {email}</p>
      </CardContent>
    </Card>
  )
}
