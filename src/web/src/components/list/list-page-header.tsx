import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

const listPageHeaderAccent: Record<
  "vehicle" | "route",
  {
    blobs: [string, string, string];
    iconTile: string;
    eyebrowLine: string;
  }
> = {
  vehicle: {
    blobs: [
      "absolute -left-24 top-0 h-64 w-64 rounded-full bg-[radial-gradient(circle,oklch(0.72_0.11_195/0.22),transparent_68%)]",
      "absolute -right-16 -top-20 h-72 w-72 rounded-full bg-[radial-gradient(circle,oklch(0.78_0.14_85/0.14),transparent_65%)]",
      "absolute bottom-0 right-1/4 h-40 w-96 rounded-full bg-[radial-gradient(circle,oklch(0.7_0.06_260/0.12),transparent_70%)]",
    ],
    iconTile:
      "rounded-2xl bg-teal-500/12 text-teal-700 shadow-sm ring-1 ring-teal-600/15 dark:bg-teal-400/10 dark:text-teal-300 dark:ring-teal-400/20",
    eyebrowLine: "from-teal-600/80 to-transparent dark:from-teal-400/70",
  },
  route: {
    blobs: [
      "absolute -left-24 top-0 h-64 w-64 rounded-full bg-[radial-gradient(circle,oklch(0.72_0.11_290/0.24),transparent_68%)]",
      "absolute -right-16 -top-20 h-72 w-72 rounded-full bg-[radial-gradient(circle,oklch(0.78_0.12_75/0.16),transparent_65%)]",
      "absolute bottom-0 right-1/4 h-40 w-96 rounded-full bg-[radial-gradient(circle,oklch(0.68_0.08_300/0.16),transparent_70%)]",
    ],
    iconTile:
      "rounded-2xl bg-violet-500/14 text-violet-800 shadow-sm ring-1 ring-violet-500/15 dark:bg-violet-400/12 dark:text-violet-200 dark:ring-violet-400/25",
    eyebrowLine: "from-violet-600/80 to-transparent dark:from-violet-400/70",
  },
};

export function ListPageHeader({
  title,
  description,
  action,
  eyebrow,
  icon,
  className,
  variant = "vehicle",
}: {
  title: string;
  description: string;
  action?: ReactNode;
  /** Short label above the title, e.g. "Fleet" or "Dispatch". */
  eyebrow?: string;
  /** Decorative icon (e.g. Lucide) shown in a tinted tile — forms & list pages. */
  icon?: ReactNode;
  className?: string;
  /** Fleet (teal) vs dispatch routes (violet/amber) — matches detail shells. */
  variant?: "vehicle" | "route";
}) {
  const accent = listPageHeaderAccent[variant];
  return (
    <div
      className={cn(
        "list-page-hero-animate relative mb-10 overflow-hidden rounded-2xl border border-border/50",
        "bg-card shadow-[0_1px_0_0_oklch(0_0_0/0.04),0_12px_40px_-16px_oklch(0.35_0.02_250/0.12)]",
        "dark:border-white/10 dark:bg-card/80 dark:shadow-[0_1px_0_0_oklch(1_0_0/0.06),0_20px_50px_-24px_oklch(0_0_0/0.45)]",
        className,
      )}
    >
      <div
        className="pointer-events-none absolute inset-0 opacity-[0.65] dark:opacity-40"
        aria-hidden
      >
        {accent.blobs.map((blobClass, i) => (
          <div key={i} className={blobClass} />
        ))}
      </div>
      <div className="relative flex flex-col gap-6 px-6 py-7 sm:flex-row sm:items-start sm:justify-between sm:px-8 sm:py-8">
        <div className="flex min-w-0 flex-1 flex-col gap-5 sm:flex-row sm:items-start sm:gap-6">
          {icon ? (
            <div
              className={cn(
                "flex h-14 w-14 shrink-0 items-center justify-center transition-transform duration-300 ease-out hover:scale-[1.03] [&_svg]:size-7",
                accent.iconTile,
              )}
              aria-hidden
            >
              {icon}
            </div>
          ) : null}
          <div className="min-w-0 max-w-2xl space-y-3">
            {eyebrow ? (
              <div className="flex items-center gap-3">
                <span
                  className={cn(
                    "h-px w-10 shrink-0 bg-linear-to-r to-transparent",
                    accent.eyebrowLine,
                  )}
                  aria-hidden
                />
                <span className="text-[11px] font-semibold uppercase tracking-[0.22em] text-muted-foreground">
                  {eyebrow}
                </span>
              </div>
            ) : null}
            <h1 className="text-balance text-3xl font-bold tracking-tight text-foreground sm:text-[2rem] sm:leading-tight">
              {title}
            </h1>
            <p className="max-w-xl text-pretty text-sm leading-relaxed text-muted-foreground sm:text-[15px]">
              {description}
            </p>
          </div>
        </div>
        {action ? (
          <div className="flex shrink-0 flex-col gap-2 sm:pt-1">{action}</div>
        ) : null}
      </div>
    </div>
  );
}
