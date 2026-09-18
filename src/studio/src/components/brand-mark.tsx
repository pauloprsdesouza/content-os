import nucleus from "@/assets/brand-nucleus.svg"

import { cn } from "@/lib/utils"

const PLATE = 72

type BrandMarkProps = {
  size?: number
  className?: string
}

export function BrandMark({ size = 32, className }: BrandMarkProps) {
  const scale = size / PLATE

  return (
    <span
      aria-hidden
      className={cn("relative inline-block shrink-0 overflow-visible", className)}
      style={{ width: size, height: size }}
    >
      <span
        className="absolute top-0 left-0 origin-top-left"
        style={{ width: PLATE, height: PLATE, transform: `scale(${scale})` }}
      >
        <span className="absolute inset-0 rounded-[14px] bg-[var(--ink)]" />
        <span
          className="absolute flex items-center justify-center"
          style={{ left: 16.71, top: -3.29, width: 40, height: 40 }}
        >
          <span
            className="-rotate-45 bg-[var(--primary)]"
            style={{ width: 28.286, height: 28.286, borderRadius: 7.714 }}
          />
        </span>
        <span
          className="absolute flex items-center justify-center"
          style={{ left: 29.57, top: 8.6, width: 29.092, height: 29.092 }}
        >
          <span
            className="-rotate-45 bg-[var(--info)]"
            style={{ width: 20.571, height: 20.571, borderRadius: 6.429 }}
          />
        </span>
        <img
          alt=""
          className="absolute block max-w-none"
          src={nucleus}
          style={{ left: 29.57, top: 29.57, width: 12.857, height: 12.857 }}
        />
      </span>
    </span>
  )
}
