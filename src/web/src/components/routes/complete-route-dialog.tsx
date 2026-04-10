"use client";

import { useCallback, useEffect, useState } from "react";
import { CheckCircle2, Loader2, Route as RouteIcon } from "lucide-react";
import { NaturalNumberInput } from "@/components/form/natural-number-input";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export function CompleteRouteDialog({
  open,
  onOpenChange,
  routeLabel,
  startMileage,
  isPending = false,
  onConfirm,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  routeLabel: string;
  startMileage: number;
  isPending?: boolean;
  onConfirm: (endMileage: number) => void | Promise<void>;
}) {
  const [endMileage, setEndMileage] = useState(startMileage);
  const [error, setError] = useState<string | null>(null);

  const resetForm = useCallback(() => {
    setEndMileage(startMileage);
    setError(null);
  }, [startMileage]);

  const closeDialog = useCallback(() => {
    resetForm();
    onOpenChange(false);
  }, [onOpenChange, resetForm]);

  useEffect(() => {
    if (!open) {
      return;
    }

    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isPending) {
        closeDialog();
      }
    };

    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [closeDialog, isPending, open]);

  useEffect(() => {
    if (!open) {
      return;
    }

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, [open]);

  useEffect(() => {
    if (open) {
      setEndMileage(startMileage);
      setError(null);
    }
  }, [open, startMileage]);

  if (!open) {
    return null;
  }

  async function handleConfirm() {
    if (endMileage < startMileage) {
      setError("End mileage cannot be lower than the route start mileage.");
      return;
    }

    setError(null);
    await onConfirm(endMileage);
    resetForm();
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
      <button
        type="button"
        aria-label="Dismiss"
        disabled={isPending}
        className="absolute inset-0 bg-zinc-950/55 backdrop-blur-[3px] transition-opacity"
        onClick={() => !isPending && closeDialog()}
      />

      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="complete-route-title"
        className={cn(
          "relative z-10 w-full max-w-lg overflow-hidden rounded-2xl border border-border/70",
          "bg-card text-card-foreground shadow-[0_25px_50px_-12px_rgba(0,0,0,0.35)]",
          "animate-in fade-in-0 zoom-in-95 duration-200",
        )}
      >
        <div className="relative border-b border-emerald-500/15 bg-linear-to-br from-emerald-500/[0.07] via-transparent to-sky-500/5 px-6 pt-6 pb-5">
          <div className="flex gap-4">
            <div
              className={cn(
                "flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl",
                "bg-emerald-500/12 text-emerald-700 ring-1 ring-emerald-500/20",
              )}
            >
              <CheckCircle2 className="size-7" strokeWidth={1.75} aria-hidden />
            </div>
            <div className="min-w-0 flex-1 space-y-1.5 pt-0.5">
              <h2
                id="complete-route-title"
                className="text-lg font-semibold leading-tight tracking-tight"
              >
                Complete this route?
              </h2>
              <p className="text-sm leading-relaxed text-muted-foreground">
                Save the final odometer reading and move this route into completed history.
              </p>
            </div>
          </div>

          <div className="mt-5 flex items-center gap-3 rounded-xl border border-border/60 bg-muted/40 px-4 py-3">
            <RouteIcon className="size-5 shrink-0 text-muted-foreground" aria-hidden />
            <div className="min-w-0">
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                Route
              </p>
              <p className="truncate font-mono text-base font-semibold tabular-nums tracking-wide">
                {routeLabel}
              </p>
            </div>
          </div>
        </div>

        <div className="space-y-4 px-6 py-5">
          <div className="rounded-xl border border-border/60 bg-muted/20 p-4 text-sm">
            <div className="flex items-center justify-between gap-3">
              <span className="text-muted-foreground">Start mileage</span>
              <span className="font-semibold">{startMileage.toLocaleString()} km</span>
            </div>
          </div>

          <div className="space-y-2">
            <label htmlFor="complete-route-mileage" className="block text-sm font-medium">
              End mileage
            </label>
            <NaturalNumberInput
              id="complete-route-mileage"
              value={endMileage}
              onChange={(value) => {
                setEndMileage(value);
                if (error) {
                  setError(null);
                }
              }}
              aria-invalid={error ? true : undefined}
            />
            {error ? <p className="text-sm text-destructive">{error}</p> : null}
          </div>
        </div>

        <div className="flex flex-col-reverse gap-2 border-t border-border/50 bg-muted/20 px-4 py-4 sm:flex-row sm:justify-end sm:gap-3 sm:px-6">
          <Button
            type="button"
            variant="outline"
            size="lg"
            disabled={isPending}
            className="w-full sm:w-auto"
            onClick={closeDialog}
          >
            Keep open
          </Button>
          <Button
            type="button"
            size="lg"
            disabled={isPending}
            className="w-full gap-2 sm:w-auto"
            onClick={() => void handleConfirm()}
          >
            {isPending ? (
              <>
                <Loader2 className="size-4 animate-spin shrink-0" aria-hidden />
                Completing...
              </>
            ) : (
              "Complete route"
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}

export default CompleteRouteDialog;
