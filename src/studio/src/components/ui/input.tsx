import * as React from "react"

import { cn } from "@/lib/utils"

function Input({ className, type, ...props }: React.ComponentProps<"input">) {
  return (
    <input
      type={type}
      className={cn(
        "flex h-11 w-full rounded-[var(--radius-sm)] border border-[var(--line)] bg-white/80 px-3 text-sm text-[var(--ink)] shadow-sm outline-none transition placeholder:text-[var(--muted-light)] focus:border-[var(--accent-strong)] focus:ring-2 focus:ring-[color-mix(in_srgb,var(--accent)_25%,transparent)] disabled:cursor-not-allowed disabled:opacity-50",
        className,
      )}
      {...props}
    />
  )
}

export { Input }
