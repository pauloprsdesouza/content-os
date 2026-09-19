import { apiRequest } from "@/lib/api/client"

export type DashboardSummary = {
  sourcesTotal: number
  claimsPendingReview: number
  researchJobsActive: number
  contentVersionsPendingApproval: number
  publicationPackagesReady: number
  purchasesConfirmed: number
  learnersActive: number
  outcomesCompleted: number
}

export function getDashboardSummary() {
  return apiRequest<DashboardSummary>("/api/v1/dashboard/summary")
}

export function decisionCount(summary: DashboardSummary) {
  return (
    summary.claimsPendingReview +
    summary.contentVersionsPendingApproval +
    summary.publicationPackagesReady
  )
}

export function reviewCount(summary: DashboardSummary) {
  return summary.claimsPendingReview + summary.contentVersionsPendingApproval
}
