import { useState, type FormEvent } from "react"
import { Navigate, useLocation, useNavigate } from "react-router"
import { toast } from "sonner"

import { BrandMark } from "@/components/brand-mark"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { apiRequest, clearCsrfToken } from "@/lib/api/client"
import { useAuth } from "@/lib/auth"

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const { session, isLoading, refreshSession } = useAuth()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const from =
    (location.state as { from?: string } | null)?.from &&
    (location.state as { from?: string }).from !== "/login"
      ? (location.state as { from: string }).from
      : "/"

  if (!isLoading && session?.isAuthenticated) {
    return <Navigate to={from} replace />
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)

    const formData = new FormData(event.currentTarget)

    try {
      clearCsrfToken()
      await apiRequest<void>("/api/v1/auth/login", {
        method: "POST",
        body: {
          email: formData.get("email"),
          password: formData.get("password"),
          rememberMe: false,
        },
      })

      clearCsrfToken()
      await refreshSession()
      toast.success("Sessão iniciada")
      navigate(from, { replace: true })
    } catch (requestError) {
      setError(
        requestError instanceof Error
          ? requestError.message
          : "O serviço de autenticação está indisponível.",
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="grid min-h-screen bg-[var(--ink)] lg:grid-cols-[1fr_500px] lg:items-center lg:gap-16 lg:px-20 xl:px-28">
      <section className="hidden text-white lg:block">
        <div className="mb-16 flex items-center gap-4">
          <BrandMark size={56} />
          <p className="m-0 text-xl font-bold tracking-[0.02em] text-white">CONTENT OS</p>
        </div>
        <h1 className="m-0 max-w-[620px] text-[42px] font-bold leading-[1.2] tracking-[-0.03em]">
          Conhecimento confiável.
          <br />
          Conteúdo que gera resultado.
        </h1>
        <p className="mb-0 mt-6 max-w-[560px] text-lg leading-relaxed text-white/70">
          Uma plataforma editorial orientada por evidências, com IA governada e revisão humana.
        </p>
      </section>

      <section className="flex min-h-screen items-center justify-center p-6 lg:min-h-0 lg:justify-end lg:p-0">
        <div className="w-full max-w-[500px] rounded-[var(--radius-xl)] bg-[var(--card)] px-10 py-12 shadow-[var(--shadow-card)] sm:px-12 sm:py-14">
          <div className="mb-8 flex items-center gap-3 lg:hidden">
            <BrandMark size={40} />
            <p className="m-0 text-base font-bold tracking-[0.02em] text-[var(--ink)]">CONTENT OS</p>
          </div>
          <h2 className="m-0 text-[30px] font-bold tracking-[-0.03em] text-[var(--foreground)]">
            Entrar
          </h2>
          <p className="mb-0 mt-2 text-sm text-[var(--muted-foreground)]">
            Acesse seu workspace do Content Studio.
          </p>
          <p className="mb-0 mt-3 text-xs text-[var(--muted-foreground)]">
            Demo local: admin@contentos.local / ChangeMe!Admin1
          </p>

          <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
            <div className="space-y-2">
              <Label htmlFor="email">E-mail</Label>
              <Input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                defaultValue="admin@contentos.local"
                placeholder="paulo@contentos.com"
                required
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Senha</Label>
              <Input
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                defaultValue="ChangeMe!Admin1"
                required
              />
            </div>

            {error && (
              <p role="alert" className="m-0 text-sm leading-relaxed text-[var(--destructive)]">
                {error}
              </p>
            )}

            <Button className="w-full" type="submit" disabled={isSubmitting}>
              {isSubmitting ? "Entrando…" : "Entrar"}
            </Button>

            <p className="mb-0 pt-2 text-center text-[11px] text-[var(--muted-foreground)]">
              Sessão segura via cookie SameSite.
            </p>
          </form>
        </div>
      </section>
    </main>
  )
}
