"use client";

import type { Dispatch, SetStateAction } from "react";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function ListPagePagination({
  page,
  totalPages,
  setPage,
  className,
}: {
  page: number;
  totalPages: number;
  setPage: Dispatch<SetStateAction<number>>;
  className?: string;
}) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <div
      className={cn(
        "mt-10 flex flex-wrap items-center justify-center gap-3",
        className,
      )}
    >
      <Button
        type="button"
        variant="outline"
        size="sm"
        onClick={() => setPage((p) => Math.max(1, p - 1))}
        disabled={page === 1}
      >
        Previous
      </Button>
      <span className="rounded-full border border-border/60 bg-muted/40 px-4 py-1.5 text-sm text-muted-foreground tabular-nums shadow-sm dark:bg-muted/25">
        Page {page} of {totalPages}
      </span>
      <Button
        type="button"
        variant="outline"
        size="sm"
        onClick={() => setPage((p) => p + 1)}
        disabled={page >= totalPages}
      >
        Next
      </Button>
    </div>
  );
}
