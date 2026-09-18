import { apiRequest, type PageResponse } from "@/lib/api/client"

export type PurchaseListItem = {
  id: string
  provider: string
  externalId: string
  status: string
  buyerEmail: string | null
  productId: string | null
  editionId: string | null
  learnerId: string | null
  webhookInboxEntryId: string | null
  reconciliationRunId: string | null
  confirmedAt: string | null
  createdAt: string
  updatedAt: string
}

export type ReconciliationRunResult = {
  runId: string
  processedCount: number
  confirmedCount: number
  ignoredCount: number
}

/** Statuses that are signals only — never label as confirmed sale. */
export const SIGNAL_PURCHASE_STATUSES = new Set([
  "SignalReceived",
  "ReconciliationQueued",
])

export function purchaseStatusLabel(status: string): string {
  switch (status) {
    case "SignalReceived":
      return "Sinal recebido (webhook)"
    case "ReconciliationQueued":
      return "Em reconciliação"
    case "Confirmed":
      return "Confirmada"
    case "Refunded":
      return "Reembolsada"
    case "Chargeback":
      return "Chargeback"
    case "Ignored":
      return "Ignorada"
    default:
      return status
  }
}

export function listPurchases(page = 1, pageSize = 25) {
  return apiRequest<PageResponse<PurchaseListItem>>(
    `/api/v1/purchases?page=${page}&pageSize=${pageSize}`,
  )
}

export function recordOrderSignal(externalId: string) {
  return apiRequest<{ purchaseId: string; alreadyExisted: boolean }>(
    "/api/v1/commerce/order-signals",
    { method: "POST", body: { externalId } },
  )
}

export function runReconciliation() {
  return apiRequest<ReconciliationRunResult>(
    "/api/v1/commerce/reconciliation-runs",
    { method: "POST" },
  )
}
