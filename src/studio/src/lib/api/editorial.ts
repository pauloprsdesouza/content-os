import { apiRequest } from "@/lib/api/client"
import type { OperationAccepted } from "@/lib/api/research"

export type TopicArea = {
  id: string
  name: string
}

export type TopicProposal = {
  id: string
  label: string
  rationale: string | null
  workIds: string[]
  isSelected: boolean
}

export type TopicDiscovery = {
  id: string
  format: string
  areaId: string
  areaName: string
  areaIsSubfield: boolean
  windowDays: number
  status: string
  operationId: string | null
  contentUnitId: string | null
  seriesId: string | null
  proposals: TopicProposal[]
}

export type EditorialSeries = {
  id: string
  format: string
  areaId: string
  areaName: string
  areaIsSubfield: boolean
  windowDays: number
  cadence: string
  nextCollectionAt: string | null
  createdAt: string
}

export function listTopicAreas(parentId?: string) {
  const query = parentId ? `?parentId=${encodeURIComponent(parentId)}` : ""
  return apiRequest<TopicArea[]>(`/api/v1/topic-areas${query}`)
}

export function startTopicDiscovery(input: {
  format: string
  areaId: string
  areaName: string
  areaIsSubfield: boolean
  windowDays: number
}) {
  return apiRequest<{ discoveryId: string; operationId: string | null; isEmpty: boolean }>(
    "/api/v1/topic-discoveries",
    {
      method: "POST",
      body: input,
      idempotencyKey: crypto.randomUUID(),
    },
  )
}

export function listTopicDiscoveries(status?: string) {
  const query = status ? `?status=${encodeURIComponent(status)}` : ""
  return apiRequest<TopicDiscovery[]>(`/api/v1/topic-discoveries${query}`)
}

export function getTopicDiscovery(discoveryId: string) {
  return apiRequest<TopicDiscovery>(`/api/v1/topic-discoveries/${discoveryId}`)
}

export function selectDiscoveredTopics(
  discoveryId: string,
  proposalIds: string[],
  productId?: string,
) {
  return apiRequest<OperationAccepted>(`/api/v1/topic-discoveries/${discoveryId}/selection`, {
    method: "POST",
    body: { proposalIds, productId: productId ?? null },
    idempotencyKey: crypto.randomUUID(),
  })
}

export function deleteEditorialSeries(seriesId: string) {
  return apiRequest<void>(`/api/v1/editorial-series/${seriesId}`, { method: "DELETE" })
}

export function listEditorialSeries() {
  return apiRequest<EditorialSeries[]>("/api/v1/editorial-series")
}

export function createEditorialSeries(input: {
  format: string
  areaId: string
  areaName: string
  areaIsSubfield: boolean
  windowDays: number
  cadence: string
}) {
  return apiRequest<{ id: string }>("/api/v1/editorial-series", {
    method: "POST",
    body: input,
    idempotencyKey: crypto.randomUUID(),
  })
}

export function collectEditorialSeries(seriesId: string) {
  return apiRequest<{ discoveryId: string; operationId: string | null; isEmpty: boolean }>(
    `/api/v1/editorial-series/${seriesId}/collections`,
    {
      method: "POST",
      idempotencyKey: crypto.randomUUID(),
    },
  )
}
