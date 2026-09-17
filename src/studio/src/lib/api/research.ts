import { apiRequest, type PageResponse } from "@/lib/api/client"

export type ResearchJobListItem = {
  id: string
  topic: string
  status: string
  updatedAt: string
}

export type ResearchJobDetail = {
  id: string
  topic: string
  scopeNotes: string | null
  status: string
  version: number
  attemptCount: number
  failureReason: string | null
  operationId: string | null
  createdAt: string
  updatedAt: string
}

export type ResearchFinding = {
  id: string
  researchJobId: string
  statement: string
  confidence: number
  createdAt: string
}

export type OperationAccepted = {
  operationId: string
  statusUrl: string
  subjectId: string | null
}

export function listResearchJobs(page = 1, pageSize = 25) {
  return apiRequest<PageResponse<ResearchJobListItem>>(
    `/api/v1/research-jobs?page=${page}&pageSize=${pageSize}`,
  )
}

export function getResearchJob(jobId: string) {
  return apiRequest<ResearchJobDetail>(`/api/v1/research-jobs/${jobId}`)
}

export function getResearchJobFindings(jobId: string) {
  return apiRequest<ResearchFinding[]>(`/api/v1/research-jobs/${jobId}/claims`)
}

export function createResearchJob(input: { topic: string; scopeNotes?: string }) {
  return apiRequest<OperationAccepted>("/api/v1/research-jobs", {
    method: "POST",
    body: {
      topic: input.topic,
      scopeNotes: input.scopeNotes ?? null,
    },
    idempotencyKey: crypto.randomUUID(),
  })
}
