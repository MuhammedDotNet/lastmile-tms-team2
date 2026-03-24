import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

/** Outer scroll + card frame for list tables (Vehicles, Routes, etc.). */
const listDataTableScrollWrapClass =
  "list-page-table-animate overflow-x-auto rounded-2xl border border-border/50 bg-card/80 shadow-[0_1px_0_0_oklch(0_0_0/0.05),0_16px_48px_-20px_oklch(0.4_0.02_250/0.14)] ring-1 ring-black/[0.04] dark:bg-card/50 dark:shadow-[0_1px_0_0_oklch(1_0_0/0.06),0_24px_56px_-28px_oklch(0_0_0/0.5)] dark:ring-white/[0.06]";

export const listDataTableHeadRowClass =
  "border-b border-border/60 bg-muted/55 text-[11px] uppercase tracking-[0.14em] backdrop-blur-[2px] dark:bg-muted/30";

export const listDataTableThClass =
  "px-4 py-3.5 text-left font-semibold text-muted-foreground first:pl-5 last:pr-5";

export const listDataTableThRightClass =
  "min-w-32 whitespace-nowrap px-3 py-3.5 text-right font-semibold text-muted-foreground last:pr-5";

export const listDataTableBodyRowClass =
  "border-b border-border/40 transition-colors odd:bg-background/80 even:bg-muted/[0.28] last:border-b-0 hover:bg-teal-500/[0.06] dark:odd:bg-transparent dark:even:bg-white/[0.03] dark:hover:bg-teal-400/[0.07]";

/** Default padding for body cells (matches thead). */
export const listDataTableTdClass =
  "min-w-0 px-4 py-3.5 align-middle first:pl-5 last:pr-5";

export function ListDataTable({
  minWidthClassName,
  children,
  className,
}: {
  /** Tailwind min-width, e.g. `min-w-[760px]` */
  minWidthClassName: string;
  children: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn(listDataTableScrollWrapClass, className)}>
      <table
        className={cn(
          "w-full text-sm text-foreground antialiased",
          minWidthClassName,
        )}
      >
        {children}
      </table>
    </div>
  );
}
