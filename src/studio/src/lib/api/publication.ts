import { apiRequest, apiRequestWithEtag } from "@/lib/api/client"

export type PublicationPackage = {
  id: string
  editionId: string
  status: string
  rendererVersion: string
  manifestJson: string | null
  exportBlobSha256: string | null
  exportBlobLength: number | null
  confirmedByUserId: string | null
  confirmedAt: string | null
  version: number
  createdAt: string
  updatedAt: string
}

export function createPublicationPackage(editionId: string) {
  return apiRequestWithEtag<PublicationPackage>(
    `/api/v1/editions/${editionId}/publication-packages`,
    {
      method: "POST",
      idempotencyKey: crypto.randomUUID(),
    },
  )
}

export function getPublicationPackage(packageId: string) {
  return apiRequestWithEtag<PublicationPackage>(
    `/api/v1/publication-packages/${packageId}`,
  )
}

export function exportPublicationPackage(packageId: string) {
  return apiRequestWithEtag<PublicationPackage>(
    `/api/v1/publication-packages/${packageId}/export`,
    {
      method: "POST",
    },
  )
}

export function confirmPublication(packageId: string, etag: string) {
  return apiRequest<void>(
    `/api/v1/publication-packages/${packageId}/confirm-publication`,
    {
      method: "POST",
      ifMatch: etag,
    },
  )
}
