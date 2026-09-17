import { apiRequest, apiRequestWithEtag, type PageResponse } from "@/lib/api/client"

export type ProductListItem = {
  id: string
  name: string
  description: string | null
  updatedAt: string
}

export type CurriculumItem = {
  position: number
  contentVersionId: string
}

export type EditionDetail = {
  id: string
  productId: string
  productName: string
  name: string
  version: number
  curriculum: CurriculumItem[]
  createdAt: string
  updatedAt: string
}

/** Matches DevelopmentSeed demo catalog ids. */
export const DEMO_PRODUCT_ID = "01999999-0001-7000-8000-000000000001"
export const DEMO_EDITION_ID = "01999999-0001-7000-8000-000000000002"

const SELECTION_KEY = "contentos.catalog.selection"

export type CatalogSelection = {
  productId: string
  editionId: string
}

export function listProducts(page = 1, pageSize = 25) {
  return apiRequest<PageResponse<ProductListItem>>(
    `/api/v1/products?page=${page}&pageSize=${pageSize}`,
  )
}

export function getEdition(productId: string, editionId: string) {
  return apiRequestWithEtag<EditionDetail>(
    `/api/v1/products/${productId}/editions/${editionId}`,
  )
}

export function replaceCurriculum(
  editionId: string,
  etag: string,
  contentVersionIds: string[],
) {
  return apiRequestWithEtag<void>(`/api/v1/editions/${editionId}/curriculum`, {
    method: "PUT",
    ifMatch: etag,
    body: { contentVersionIds },
  })
}

export function readCatalogSelection(): CatalogSelection | null {
  try {
    const raw = sessionStorage.getItem(SELECTION_KEY)
    if (!raw) {
      return null
    }
    const parsed = JSON.parse(raw) as CatalogSelection
    if (!parsed.productId || !parsed.editionId) {
      return null
    }
    return parsed
  } catch {
    return null
  }
}

export function writeCatalogSelection(selection: CatalogSelection) {
  sessionStorage.setItem(SELECTION_KEY, JSON.stringify(selection))
}
