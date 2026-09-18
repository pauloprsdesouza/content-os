import { apiRequest, apiRequestWithEtag, type PageResponse } from "@/lib/api/client"
import type { OperationAccepted } from "@/lib/api/research"

export type ContentUnitListItem = {
  id: string
  title: string
  brief: string | null
  format: string
  latestVersionId: string | null
  latestVersionStatus: string | null
  updatedAt: string
  productId: string | null
}

export type ContentVersionDetail = {
  id: string
  contentUnitId: string
  revision: number
  bodyMarkdown: string | null
  status: string
  version: number
  agentReviewNotes: string | null
  changeRequestNotes: string | null
  reviewedByUserId: string | null
  reviewedAt: string | null
  operationId: string | null
  createdAt: string
  updatedAt: string
}

export function listContentUnits(page = 1, pageSize = 25, productId?: string) {
  const product = productId ? `&productId=${productId}` : ""
  return apiRequest<PageResponse<ContentUnitListItem>>(
    `/api/v1/content-units?page=${page}&pageSize=${pageSize}${product}`,
  )
}

export function deleteContentUnit(contentUnitId: string) {
  return apiRequest<void>(`/api/v1/content-units/${contentUnitId}`, { method: "DELETE" })
}

export function createContentUnit(input: {
  title: string
  brief?: string
  format: string
  queueGeneration?: boolean
  citationContentHashes?: string[]
  productId?: string
}) {
  return apiRequest<OperationAccepted | { id: string; versionId: string }>(
    "/api/v1/content-units",
    {
      method: "POST",
      body: {
        title: input.title,
        brief: input.brief ?? null,
        format: input.format,
        queueGeneration: input.queueGeneration ?? true,
        citationContentHashes: input.citationContentHashes ?? null,
        productId: input.productId ?? null,
      },
      idempotencyKey: crypto.randomUUID(),
    },
  )
}

export function getContentVersion(versionId: string) {
  return apiRequestWithEtag<ContentVersionDetail>(`/api/v1/content-versions/${versionId}`)
}

export function updateContentVersion(versionId: string, etag: string, bodyMarkdown: string) {
  return apiRequestWithEtag<ContentVersionDetail>(`/api/v1/content-versions/${versionId}`, {
    method: "PUT",
    ifMatch: etag,
    body: { bodyMarkdown },
  })
}

export function requestContentReview(versionId: string) {
  return apiRequest<OperationAccepted>(
    `/api/v1/content-versions/${versionId}/request-review`,
    {
      method: "POST",
      idempotencyKey: crypto.randomUUID(),
    },
  )
}

export function approveContentVersion(versionId: string, etag: string) {
  return apiRequest<void>(`/api/v1/content-versions/${versionId}/approve`, {
    method: "POST",
    ifMatch: etag,
  })
}

export function requestContentChanges(versionId: string, etag: string, notes: string) {
  return apiRequest<void>(`/api/v1/content-versions/${versionId}/request-changes`, {
    method: "POST",
    ifMatch: etag,
    body: { notes },
  })
}
