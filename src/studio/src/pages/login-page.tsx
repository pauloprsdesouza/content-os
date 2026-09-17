import { useState, type FormEvent } from "react"
import { ArrowRight, ShieldCheck, Sparkles } from "lucide-react"
import { useNavigate } from "react-router"
import { toast } from "sonner"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

export function LoginPage() {
  const navigate = useNavigate()
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError(null)
    setIsSubmitting(true)

    const formData = new FormData(event.currentTarget)

    try {
      const response = await fetch("/api/v1/auth/login", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          email: formData.get("email"),
          password: formData.get("password"),
        }),
      })

      if (!response.ok) {
        throw new Error("Não foi possível entrar. Verifique seus dados e tente novamente.")
      }

      toast.success("Sessão iniciada")
      navigate("/")
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
    <main className="grid min-h-screen lg:grid-cols-[1.05fr_0.95fr]">
      <section className="relative hidden overflow-hidden bg-[var(--ink)] p-14 text-white lg:flex lg:flex-col lg:justify-between">
        <div className="absolute inset-0 opacity-30 dot-grid" />
        <div className="relative flex items-center gap-3">
          <div className="grid size-11 place-items-center rounded-xl bg-[var(--accent)] text-[var(--accent-ink)]">
            <Sparkles className="size-5" />
          </div>
          <span className="brand-wordmark text-3xl font-bold tracking-[-0.04em]">Content OS</span>
        </div>
        <div className="relative max-w-xl">
          <p className="mb-5 text-xs font-bold uppercase tracking-[0.22em] text-[var(--accent)]">
            Conteúdo com procedência
          </p>
          <h1 className="brand-wordmark m-0 text-6xl leading-[1.02] tracking-[-0.045em]">
            Clareza operacional para ideias que importam.
          </h1>
          <p className="mb-0 mt-7 max-w-lg text-base leading-relaxed text-white/55">
            Um sistema para pesquisar, validar, criar e publicar sem perder a origem de cada decisão.
          </p>
        </div>
        <div className="relative flex items-center gap-2 text-xs text-white/40">
          <ShieldCheck className="size-4 text-[var(--accent)]" />
          Acesso interno protegido
        </div>
      </section>

      <section className="flex items-center justify-center p-6 md:p-12">
        <Card className="w-full max-w-md bg-white/75 backdrop-blur-xl">
          <CardHeader className="pb-4">
            <CardTitle className="text-2xl">Acesse o Studio</CardTitle>
            <CardDescription>Use suas credenciais de operação.</CardDescription>
          </CardHeader>
          <CardContent>
            <form className="space-y-5" onSubmit={handleSubmit}>
              <div className="space-y-2">
                <Label htmlFor="email">E-mail</Label>
                <Input id="email" name="email" type="email" autoComplete="email" required />
              </div>
              <div className="space-y-2">
                <Label htmlFor="password">Senha</Label>
                <Input
                  id="password"
                  name="password"
                  type="password"
                  autoComplete="current-password"
                  required
                />
              </div>
              {error && (
                <p role="alert" className="m-0 text-sm leading-relaxed text-[var(--danger)]">
                  {error}
                </p>
              )}
              <Button className="w-full" type="submit" disabled={isSubmitting}>
                {isSubmitting ? "Entrando…" : "Entrar"}
                {!isSubmitting && <ArrowRight className="size-4" />}
              </Button>
            </form>
          </CardContent>
        </Card>
      </section>
    </main>
  )
}
