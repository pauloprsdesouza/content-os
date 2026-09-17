import { apiRequest, type PageResponse } from "@/lib/api/client"

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

export function getOperation(operationId: string) {
  return apiRequest<OperationDetail>(`/api/v1/operations/${operationId}`)
}

export async function pollOperationUntilSettled(
  operationId: string,
  options?: { intervalMs?: number; maxAttempts?: number },
) {
  const intervalMs = options?.intervalMs ?? 1500
  const maxAttempts = options?.maxAttempts ?? 40

  for (let attempt = 0; attempt < maxAttempts; attempt++) {
    const operation = await getOperation(operationId)
    if (
      operation.status === "Succeeded" ||
      operation.status === "Failed" ||
      operation.status === "Cancelled"
    ) {
      return operation
    }
    await new Promise((resolve) => setTimeout(resolve, intervalMs))
  }

  return getOperation(operationId)
}
