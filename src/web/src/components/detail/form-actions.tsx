import type { ReactNode } from "react";

import { cn } from "@/lib/utils";

/** Primary / secondary actions at the end of a form — full width on mobile, row on desktop. */
export function FormActionsBar({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <div
      className={cn(
        "mt-10 flex flex-col gap-3 border-t border-border/55 pt-8 sm:flex-row sm:flex-wrap sm:items-center sm:justify-end sm:gap-3",
        className,
      )}
    >
      {children}
    </div>
  );
}
