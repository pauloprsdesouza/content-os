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
  traceId?: string
  errors?: Record<string, string[]>
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

function readString(value: unknown) {
  return typeof value === "string" ? value : undefined
}

function userFacingMessage(status: number, problem: ApiProblem) {
  if (status === 0) {
    return "Sem conexão. O que você digitou permanece na tela."
  }
  if (status === 403) {
    return "Você não tem permissão para esta ação."
  }
  if (status === 404) {
    return "Recurso não encontrado."
  }
  if (status === 409) {
    return problem.detail ?? "O estado mudou. Atualize os dados e tente de novo."
  }
  if (status === 412) {
    return "Os dados estão desatualizados. Atualize antes de tentar de novo."
  }
  if (status === 422) {
    return problem.detail ?? "Há campos inválidos."
  }
  if (status === 429) {
    return "Muitas tentativas. Aguarde antes de tentar de novo."
  }
  if (status >= 500) {
    return problem.traceId
      ? `Falha no serviço. Correlação ${problem.traceId}.`
      : "Falha no serviço. Tente de novo."
  }
  return problem.detail ?? problem.title ?? `HTTP ${status}`
}

async function readProblem(response: Response): Promise<ApiProblem> {
  const fallback: ApiProblem = { title: response.statusText, status: response.status }
  try {
    const payload = (await response.json()) as Record<string, unknown>
    const errors =
      payload.errors && typeof payload.errors === "object"
        ? (payload.errors as Record<string, string[]>)
        : undefined
    return {
      title: readString(payload.title) ?? fallback.title,
      detail: readString(payload.detail),
      status: typeof payload.status === "number" ? payload.status : response.status,
      code: readString(payload.code),
      currentVersion: typeof payload.currentVersion === "number" ? payload.currentVersion : undefined,
      traceId: readString(payload.traceId),
      errors,
    }
  } catch {
    return fallback
  }
}

function fail(response: Response, problem: ApiProblem): never {
  const message = userFacingMessage(response.status, problem)
  throw new ApiError(response.status, { ...problem, detail: problem.detail ?? message, title: message })
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

    fail(response, await readProblem(response))
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
    fail(response, await readProblem(response))
  }

  const data = response.status === 204 ? (undefined as T) : ((await response.json()) as T)
  return { data, etag: response.headers.get("ETag") }
}
