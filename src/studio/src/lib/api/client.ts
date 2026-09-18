export type PageResponse<T> = {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export type ApiProblem = {
  title?: string
  detail?: string
  status?: number
  code?: string
  currentVersion?: number
}

export class ApiError extends Error {
  readonly status: number
  readonly problem: ApiProblem

  constructor(status: number, problem: ApiProblem) {
    super(problem.detail ?? problem.title ?? `HTTP ${status}`)
    this.name = "ApiError"
    this.status = status
    this.problem = problem
  }
}

let csrfToken: string | null = null

async function ensureCsrfToken(): Promise<string> {
  if (csrfToken) {
    return csrfToken
  }

  const response = await fetch("/api/v1/auth/antiforgery", {
    credentials: "include",
  })

  if (!response.ok) {
    throw new ApiError(response.status, { title: "Falha ao obter token antiforgery" })
  }

  const payload = (await response.json()) as { token: string }
  csrfToken = payload.token
  return csrfToken
}

export function clearCsrfToken() {
  csrfToken = null
}

type RequestOptions = {
  method?: string
  body?: unknown
  ifMatch?: string
  idempotencyKey?: string
  auth?: boolean
}

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const method = options.method ?? "GET"
  const headers: Record<string, string> = {
    Accept: "application/json",
  }

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json"
  }

  if (method !== "GET" && method !== "HEAD") {
    headers["X-CSRF-TOKEN"] = await ensureCsrfToken()
  }

  if (options.ifMatch) {
    headers["If-Match"] = options.ifMatch
  }

  if (options.idempotencyKey) {
    headers["Idempotency-Key"] = options.idempotencyKey
  }

  const response = await fetch(path, {
    method,
    credentials: "include",
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (response.status === 204) {
    return undefined as T
  }

  if (!response.ok) {
    if (response.status === 401 && !path.includes("/auth/login") && !path.includes("/auth/session")) {
      clearCsrfToken()
      if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
        window.location.assign(`/login?from=${encodeURIComponent(window.location.pathname)}`)
      }
    }

    let problem: ApiProblem = { title: response.statusText, status: response.status }
    try {
      const payload = (await response.json()) as Record<string, unknown>
      problem = {
        title: typeof payload.title === "string" ? payload.title : problem.title,
        detail: typeof payload.detail === "string" ? payload.detail : undefined,
        status: typeof payload.status === "number" ? payload.status : response.status,
        code: typeof payload.code === "string" ? payload.code : undefined,
        currentVersion:
          typeof payload.currentVersion === "number" ? payload.currentVersion : undefined,
      }
    } catch {
      // keep fallback problem
    }
    throw new ApiError(response.status, problem)
  }

  if (response.headers.get("content-type")?.includes("application/json")) {
    return (await response.json()) as T
  }

  return undefined as T
}

export async function apiRequestWithEtag<T>(
  path: string,
  options: RequestOptions = {},
): Promise<{ data: T; etag: string | null }> {
  const method = options.method ?? "GET"
  const headers: Record<string, string> = {
    Accept: "application/json",
  }

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json"
  }

  if (method !== "GET" && method !== "HEAD") {
    headers["X-CSRF-TOKEN"] = await ensureCsrfToken()
  }

  if (options.ifMatch) {
    headers["If-Match"] = options.ifMatch
  }

  if (options.idempotencyKey) {
    headers["Idempotency-Key"] = options.idempotencyKey
  }

  const response = await fetch(path, {
    method,
    credentials: "include",
    headers,
    body: options.body === undefined ? undefined : JSON.stringify(options.body),
  })

  if (!response.ok) {
    let problem: ApiProblem = { title: response.statusText, status: response.status }
    try {
      problem = (await response.json()) as ApiProblem
    } catch {
      // ignore
    }
    throw new ApiError(response.status, problem)
  }

  const data = response.status === 204 ? (undefined as T) : ((await response.json()) as T)
  return { data, etag: response.headers.get("ETag") }
}
