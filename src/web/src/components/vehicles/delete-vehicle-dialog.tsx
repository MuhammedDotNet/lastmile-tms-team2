"use client";

import { useEffect } from "react";
import { AlertTriangle, Loader2, Truck } from "lucide-react";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export type DeleteVehicleDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  registrationPlate: string;
  onConfirm: () => void | Promise<void>;
  isPending?: boolean;
};

export function DeleteVehicleDialog({
  open,
  onOpenChange,
  registrationPlate,
  onConfirm,
  isPending,
}: DeleteVehicleDialogProps) {
  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape" && !isPending) onOpenChange(false);
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [open, isPending, onOpenChange]);

  useEffect(() => {
    if (open) {
      const prev = document.body.style.overflow;
      document.body.style.overflow = "hidden";
      return () => {
        document.body.style.overflow = prev;
      };
    }
  }, [open]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6">
      <button
        type="button"
        aria-label="Dismiss"
        disabled={isPending}
        className="absolute inset-0 bg-zinc-950/55 backdrop-blur-[3px] transition-opacity"
        onClick={() => !isPending && onOpenChange(false)}
      />

      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="delete-vehicle-title"
        className={cn(
          "relative z-10 w-full max-w-md overflow-hidden rounded-2xl border border-border/70",
          "bg-card text-card-foreground shadow-[0_25px_50px_-12px_rgba(0,0,0,0.35)]",
          "animate-in fade-in-0 zoom-in-95 duration-200",
        )}
      >
        <div className="relative border-b border-destructive/15 bg-linear-to-br from-destructive/[0.07] via-transparent to-amber-500/4 px-6 pt-6 pb-5">
          <div className="flex gap-4">
            <div
              className={cn(
                "flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl",
                "bg-destructive/12 text-destructive ring-1 ring-destructive/20",
              )}
            >
              <AlertTriangle className="size-7" strokeWidth={1.75} aria-hidden />
            </div>
            <div className="min-w-0 flex-1 space-y-1.5 pt-0.5">
              <h2
                id="delete-vehicle-title"
                className="text-lg font-semibold leading-tight tracking-tight"
              >
                Delete this vehicle?
              </h2>
              <p className="text-sm leading-relaxed text-muted-foreground">
                This action cannot be undone. Routes and history referencing this
                unit may be affected.
              </p>
            </div>
          </div>

          <div className="mt-5 flex items-center gap-3 rounded-xl border border-border/60 bg-muted/40 px-4 py-3">
            <Truck className="size-5 shrink-0 text-muted-foreground" aria-hidden />
            <div className="min-w-0">
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                Registration
              </p>
              <p className="truncate font-mono text-base font-semibold tabular-nums tracking-wide">
                {registrationPlate}
              </p>
            </div>
          </div>
        </div>

        <div className="flex flex-col-reverse gap-2 border-t border-border/50 bg-muted/20 px-4 py-4 sm:flex-row sm:justify-end sm:gap-3 sm:px-6">
          <Button
            type="button"
            variant="outline"
            size="lg"
            disabled={isPending}
            className="w-full sm:w-auto"
            onClick={() => onOpenChange(false)}
          >
            Cancel
          </Button>
          <Button
            type="button"
            variant="destructive"
            size="lg"
            disabled={isPending}
            className="w-full gap-2 sm:w-auto"
            onClick={() => void onConfirm()}
          >
            {isPending ? (
              <>
                <Loader2 className="size-4 animate-spin shrink-0" aria-hidden />
                Deleting…
              </>
            ) : (
              "Delete vehicle"
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
