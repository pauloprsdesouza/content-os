import { apiRequest } from "@/lib/api/client"

export type OutcomesSummary = {
  confirmedPurchases: number
  learners: number
  enrollments: number
  capstonesSubmitted: number
  evaluationsPassed: number
  outcomesRecorded: number
}

export function getOutcomesSummary() {
  return apiRequest<OutcomesSummary>("/api/v1/outcomes/summary")
}
