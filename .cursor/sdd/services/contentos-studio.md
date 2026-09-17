# Service — Content Studio (`src/studio`)

Status: Active · Runtime: React 19 + Vite + TypeScript strict

## Owns

pt-BR admin UX: dashboard, sources, research monitor, claim review, content editor, edition builder, publication, commerce views, outcomes, admin users/integrations. Design tokens/CSS vars from Figma (`ws9h9RE7NGrE9hVbwqRnxY`).

## Stack

Refine Core **headless** · shadcn/ui new-york · Tailwind CSS v4 · Orval (only HTTP client) · TanStack Query/Table · RHF + Zod · Lucide · Sonner · CVA.

## Design tokens (Figma → CSS)

Collection **Content OS / Semantic tokens** (Light/Dark). Font: **Inter**.

| Figma | CSS | Notes |
|-------|-----|--------|
| `color/bg` | `--background` | `#f7f8fc` light |
| `color/surface` | `--card` | |
| `color/ink` | `--ink` / `--sidebar` | `#0b1020` |
| `color/text` | `--foreground` | |
| `color/text-muted` | `--muted-foreground` | |
| `color/border` | `--border` | |
| `color/primary` | `--primary` | `#5b5ff2` |
| `color/primary-soft` | `--accent` | |
| `color/research` | `--info` | brand cyan / STUDIO mark |
| `color/success` / `warning` / `danger` | `--success` / `--warning` / `--destructive` | |
| `radius/sm\|md\|lg` | `--radius-*` | 8 / 12 / 16 |
| `space/1…12` | `--space-*` | 4…48 |
| `control/height` | `--control-height` | 40 |

Shell: **CONTENT / STUDIO** wordmark, 240px sidebar, top search “Buscar em todo o workspace”. Nav modules: Visão geral, Conhecimento, Conteúdo, Produtos, Publicação, Vendas, Resultados.

## Rules

- No Material UI. No hand-written platform `fetch`/Axios (login stub exception until Orval).
- Retain ETags → `If-Match`. One `Idempotency-Key` per user intent.
- Server authoritative; Zod = early feedback. Map 422 field paths.
- Role-aware nav; API remains authoritative for AuthZ.
- Figma is visual SoT — implement screens from design, not invent layouts.

## Does not own

Business totals (use `/dashboard/summary`), commerce formulas, publication confirmation logic.
