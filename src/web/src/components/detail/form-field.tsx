import { type ReactNode } from "react";

import { Label } from "@/components/ui/label";
import { cn } from "@/lib/utils";

export function DetailFormField({
  label,
  htmlFor,
  children,
  description,
  error,
  className,
}: {
  label: string;
  htmlFor?: string;
  children: ReactNode;
  description?: string;
  /** Shown instead of `description` when set. */
  error?: string;
  className?: string;
}) {
  return (
    <div className={cn("space-y-2.5", className)}>
      <div
        className={cn(
          "flex items-center gap-2.5",
          error && "text-destructive",
        )}
      >
        <span
          className={cn(
            "h-1.5 w-1.5 shrink-0 rounded-full",
            error
              ? "bg-destructive shadow-[0_0_0_3px_oklch(0.55_0.22_25/0.35)]"
              : "bg-teal-600/90 shadow-[0_0_0_3px_oklch(0.72_0.1_180/0.22)] dark:bg-teal-400/90 dark:shadow-[0_0_0_3px_oklch(0.35_0.08_180/0.35)]",
          )}
          aria-hidden
        />
        <Label
          id={htmlFor ? `${htmlFor}-label` : undefined}
          htmlFor={htmlFor}
          className={cn(
            "text-sm font-semibold leading-none",
            error ? "text-destructive" : "text-foreground",
          )}
        >
          {label}
        </Label>
      </div>
      {/* Stable wrapper: toggling error must not remount inputs (focus loss). */}
      <div
        className={cn(
          error &&
            "rounded-xl ring-2 ring-destructive/60 ring-offset-2 ring-offset-background",
        )}
      >
        {children}
      </div>
      {error ? (
        <p role="alert" className="text-xs leading-relaxed text-destructive">
          {error}
        </p>
      ) : description ? (
        <p className="text-xs leading-relaxed text-muted-foreground/90">
          {description}
        </p>
      ) : null}
    </div>
  );
}
