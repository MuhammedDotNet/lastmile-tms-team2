import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

const accentMap = {
  teal: {
    bar: "border-l-teal-500 dark:border-l-teal-400",
    icon: "bg-teal-500/[0.12] text-teal-800 ring-teal-600/15 dark:bg-teal-400/10 dark:text-teal-300 dark:ring-teal-400/20",
  },
  sky: {
    bar: "border-l-sky-500 dark:border-l-sky-400",
    icon: "bg-sky-500/[0.12] text-sky-900 ring-sky-600/15 dark:bg-sky-400/10 dark:text-sky-200 dark:ring-sky-400/20",
  },
  amber: {
    bar: "border-l-amber-500 dark:border-l-amber-400",
    icon: "bg-amber-500/[0.14] text-amber-950 ring-amber-600/15 dark:bg-amber-400/12 dark:text-amber-100 dark:ring-amber-400/25",
  },
  violet: {
    bar: "border-l-violet-500 dark:border-l-violet-400",
    icon: "bg-violet-500/[0.11] text-violet-950 ring-violet-600/15 dark:bg-violet-400/10 dark:text-violet-200 dark:ring-violet-400/20",
  },
} as const;

export type ListStatAccent = keyof typeof accentMap;

export function ListStatCard({
  label,
  children,
  hint,
  className,
  icon,
  accent = "teal",
}: {
  label: string;
  children: ReactNode;
  hint?: ReactNode;
  className?: string;
  icon?: ReactNode;
  accent?: ListStatAccent;
}) {
  const tones = accentMap[accent];

  return (
    <div
      className={cn(
        "group relative overflow-hidden rounded-xl border border-border/45 bg-card/90 shadow-sm",
        "border-l-[3px] transition-all duration-200 ease-out",
        "hover:-translate-y-0.5 hover:border-border/60 hover:shadow-md",
        "dark:bg-card/55 dark:hover:border-white/12",
        tones.bar,
        className,
      )}
    >
      <div className="relative flex gap-3.5 px-4 py-3.5">
        {icon != null ? (
          <div
            className={cn(
              "flex h-11 w-11 shrink-0 items-center justify-center rounded-xl ring-1",
              "[&_svg]:size-[1.35rem]",
              tones.icon,
            )}
            aria-hidden
          >
            {icon}
          </div>
        ) : null}
        <div className="min-w-0 flex-1">
          <p className="text-[11px] font-semibold uppercase tracking-widest text-muted-foreground">
            {label}
          </p>
          <div className="mt-1.5">{children}</div>
          {hint != null ? (
            <p className="mt-1.5 text-xs leading-relaxed text-muted-foreground/85">
              {hint}
            </p>
          ) : null}
        </div>
      </div>
    </div>
  );
}
