import { apiFormRequest, apiRequest, apiRequestWithEtag, type PageResponse } from "@/lib/api/client"

export type SourceListItem = {
  id: string
  displayName: string
  canonicalUri: string
  kind: string
  updatedAt: string
}

export type SourceDetail = {
  id: string
  displayName: string
  canonicalUri: string
  kind: string
  createdAt: string
  updatedAt: string
}

export type SnapshotItem = {
  id: string
  sourceId: string
  contentHash: string
  mediaType: string
  byteLength: number
  capturedAt: string
}

export type ClaimQueueItem = {
  id: string
  statement: string
  status: string
  confidence: number
  version: number
  updatedAt: string
}

export type ClaimEvidence = {
  evidenceId: string
  snapshotId: string
  locator: string
  extractionMethod: string
  confidence: number
}

export type ClaimDetail = {
  id: string
  statement: string
  status: string
  confidence: number
  version: number
  rejectionReason: string | null
  reviewedByUserId: string | null
  reviewedAt: string | null
  createdAt: string
  updatedAt: string
  evidence: ClaimEvidence[]
}

export function listSources(page = 1, pageSize = 25) {
  return apiRequest<PageResponse<SourceListItem>>(
    `/api/v1/sources?page=${page}&pageSize=${pageSize}`,
  )
}

export function getSource(sourceId: string) {
  return apiRequest<SourceDetail>(`/api/v1/sources/${sourceId}`)
}

export function createSource(input: {
  location: string
  kind: string
  displayName: string
}) {
  return apiRequest<{ id: string }>("/api/v1/sources", {
    method: "POST",
    body: input,
    idempotencyKey: crypto.randomUUID(),
  })
}

export function listSnapshots(sourceId: string, page = 1, pageSize = 25) {
  return apiRequest<PageResponse<SnapshotItem>>(
    `/api/v1/sources/${sourceId}/snapshots?page=${page}&pageSize=${pageSize}`,
  )
}

export type CaptureResult = {
  sourceId: string
  snapshotId: string
  contentHash: string
  sourceCreated: boolean
}

export function captureSource(input: {
  mode: "web" | "text"
  displayName?: string
  location?: string
  text?: string
}) {
  return apiRequest<CaptureResult>("/api/v1/source-captures", {
    method: "POST",
    body: input,
    idempotencyKey: crypto.randomUUID(),
  })
}

export function captureExistingSource(
  sourceId: string,
  input: { mode: "web" | "text"; text?: string },
) {
  return apiRequest<CaptureResult>(`/api/v1/sources/${sourceId}/captures`, {
    method: "POST",
    body: input,
    idempotencyKey: crypto.randomUUID(),
  })
}

export function uploadSourceFile(displayName: string, file: File) {
  const body = new FormData()
  body.set("displayName", displayName)
  body.set("file", file)
  return apiFormRequest<CaptureResult>("/api/v1/source-captures/files", body)
}

export function createSnapshot(
  sourceId: string,
  input: { contentBase64: string; mediaType: string },
) {
  return apiRequest<{ id: string; contentHash: string }>(
    `/api/v1/sources/${sourceId}/snapshots`,
    {
      method: "POST",
      body: input,
      idempotencyKey: crypto.randomUUID(),
    },
  )
}

export function listPendingClaims(page = 1, pageSize = 25) {
  return apiRequest<PageResponse<ClaimQueueItem>>(
    `/api/v1/claims?page=${page}&pageSize=${pageSize}`,
  )
}

export function getClaim(claimId: string) {
  return apiRequestWithEtag<ClaimDetail>(`/api/v1/claims/${claimId}`)
}

export function approveClaim(claimId: string, etag: string) {
  return apiRequest<void>(`/api/v1/claims/${claimId}/approve`, {
    method: "POST",
    ifMatch: etag,
    idempotencyKey: crypto.randomUUID(),
  })
}

export function rejectClaim(claimId: string, etag: string, reason: string) {
  return apiRequest<void>(`/api/v1/claims/${claimId}/reject`, {
    method: "POST",
    ifMatch: etag,
    body: { reason },
    idempotencyKey: crypto.randomUUID(),
  })
}
