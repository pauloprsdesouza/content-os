import { Navigate, Outlet, useLocation } from "react-router"

import { useAuth } from "@/lib/auth"

export function RequireAuth() {
  const { session, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return (
      <div className="grid min-h-screen place-items-center text-sm text-[var(--muted-foreground)]">
        Verificando sessão…
      </div>
    )
  }

  if (!session?.isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />
  }

  return <Outlet />
}
