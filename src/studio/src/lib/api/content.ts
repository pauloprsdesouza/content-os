import { apiRequest, apiRequestWithEtag, type PageResponse } from "@/lib/api/client"
import type { OperationAccepted } from "@/lib/api/research"

export type ContentUnitListItem = {
  id: string
  title: string
  brief: string | null
  latestVersionId: string | null
  latestVersionStatus: string | null
  updatedAt: string
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

export function listContentUnits(page = 1, pageSize = 25) {
  return apiRequest<PageResponse<ContentUnitListItem>>(
    `/api/v1/content-units?page=${page}&pageSize=${pageSize}`,
  )
}

export function createContentUnit(input: {
  title: string
  brief?: string
  queueGeneration?: boolean
}) {
  return apiRequest<OperationAccepted | { id: string; versionId: string }>(
    "/api/v1/content-units",
    {
      method: "POST",
      body: {
        title: input.title,
        brief: input.brief ?? null,
        queueGeneration: input.queueGeneration ?? true,
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
