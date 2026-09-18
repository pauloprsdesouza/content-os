import { ApiError, type ApiProblem } from "@/lib/api/client"

export type OperationDetail = {
  id: string
  kind: string
  subjectType: string
  subjectId: string
  status: string
  errorMessage: string | null
  createdAt: string
  updatedAt: string
}

const terminalStatuses = new Set(["Succeeded", "Failed", "Cancelled"])

export function watchOperation(
  operationId: string,
  onUpdate: (operation: OperationDetail) => void,
  signal?: AbortSignal,
): Promise<OperationDetail> {
  return new Promise((resolve, reject) => {
    if (signal?.aborted) {
      reject(new DOMException("Aborted", "AbortError"))
      return
    }

    const source = new EventSource(`/api/v1/operations/${operationId}/events`, {
      withCredentials: true,
    })
    let settled = false

    const finish = (error?: Error, operation?: OperationDetail) => {
      if (settled) {
        return
      }
      settled = true
      source.close()
      signal?.removeEventListener("abort", onAbort)
      if (error) {
        reject(error)
        return
      }
      resolve(operation as OperationDetail)
    }

    const onAbort = () => finish(new DOMException("Aborted", "AbortError"))
    signal?.addEventListener("abort", onAbort, { once: true })

    source.addEventListener("operation", (message) => {
      const operation = JSON.parse((message as MessageEvent<string>).data) as OperationDetail
      onUpdate(operation)
      if (terminalStatuses.has(operation.status)) {
        finish(undefined, operation)
      }
    })

    source.onerror = () => {
      if (source.readyState === EventSource.CLOSED) {
        const problem: ApiProblem = { title: "A conexão de status caiu." }
        finish(new ApiError(0, problem))
      }
    }
  })
}
