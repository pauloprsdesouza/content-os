# Service — Content Studio (`src/studio`)

Status: Active · Runtime: React 19 + Vite + TypeScript strict

## Owns

pt-BR admin UX: dashboard, sources, research monitor, claim review, content editor, edition builder, publication, commerce views, outcomes, admin users/integrations. Design tokens/CSS vars from Figma.

## Stack

Refine Core **headless** · shadcn/ui new-york · Tailwind CSS v4 · Orval (only HTTP client) · TanStack Query/Table · RHF + Zod · Lucide · Sonner · CVA.

## Rules

- No Material UI. No hand-written platform `fetch`/Axios.
- Retain ETags → `If-Match`. One `Idempotency-Key` per user intent.
- Server authoritative; Zod = early feedback. Map 422 field paths.
- Role-aware nav; API remains authoritative for AuthZ.
- Figma is visual SoT — implement screens from design, not invent layouts.

## Does not own

Business totals (use `/dashboard/summary`), commerce formulas, publication confirmation logic.
