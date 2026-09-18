const labels: Record<string, string> = {
  Collecting: "coletando temas",
  AwaitingSelection: "escolha os temas",
  Writing: "escrevendo",
  ReadyForReview: "pronto para revisar",
  GenerationQueued: "escrevendo",
  ReviewQueued: "escrevendo",
  Generated: "pronto para revisar",
  AgentReviewed: "pronto para revisar",
  PendingHumanApproval: "pronto para revisar",
  ChangesRequested: "pronto para revisar",
  Draft: "rascunho",
  Approved: "aprovado",
}

export function taskStatus(status: string | null | undefined) {
  if (!status) {
    return "sem versão"
  }
  return labels[status] ?? status
}

export const contentFormats = [
  { code: "newsletter", label: "Newsletter", detail: "Assunto, abertura, três pontos e o que fazer." },
  { code: "post", label: "Post", detail: "Gancho, uma tese e um fechamento." },
  { code: "aula", label: "Aula", detail: "Objetivo, blocos e um exercício." },
  { code: "ebook", label: "Ebook", detail: "Sumário e capítulos com evidência." },
  { code: "artigo", label: "Artigo", detail: "Tese, desenvolvimento e limitações." },
] as const
