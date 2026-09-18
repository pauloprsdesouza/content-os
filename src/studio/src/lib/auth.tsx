import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react"

import { apiRequest, clearCsrfToken } from "@/lib/api/client"

export type AuthSession = {
  isAuthenticated: boolean
  userName: string | null
}

type AuthContextValue = {
  session: AuthSession | null
  isLoading: boolean
  refreshSession: () => Promise<AuthSession>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

async function fetchSession(): Promise<AuthSession> {
  const payload = await apiRequest<{ isAuthenticated: boolean; userName: string | null }>(
    "/api/v1/auth/session",
  )
  return {
    isAuthenticated: Boolean(payload.isAuthenticated),
    userName: payload.userName ?? null,
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthSession | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const refreshSession = useCallback(async () => {
    const next = await fetchSession()
    setSession(next)
    setIsLoading(false)
    return next
  }, [])

  const logout = useCallback(async () => {
    try {
      clearCsrfToken()
      await apiRequest<void>("/api/v1/auth/logout", { method: "POST" })
    } catch {
      // Always clear local auth state even if the cookie was already invalid.
    } finally {
      clearCsrfToken()
      setSession({ isAuthenticated: false, userName: null })
    }
  }, [])

  useEffect(() => {
    void refreshSession().catch(() => {
      setSession({ isAuthenticated: false, userName: null })
      setIsLoading(false)
    })
  }, [refreshSession])

  const value = useMemo(
    () => ({ session, isLoading, refreshSession, logout }),
    [session, isLoading, refreshSession, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider")
  }
  return context
}
