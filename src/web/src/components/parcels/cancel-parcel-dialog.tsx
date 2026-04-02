"use client";

import { useCallback, useEffect, useState } from "react";
import { AlertTriangle, Loader2, PackageX } from "lucide-react";

import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

export type CancelParcelDialogProps = {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  trackingNumber: string;
  onConfirm: (reason: string) => void | Promise<void>;
  isPending?: boolean;
};

export function CancelParcelDialog({
  open,
  onOpenChange,
  trackingNumber,
  onConfirm,
  isPending = false,
}: CancelParcelDialogProps) {
  const [reason, setReason] = useState("");
  const [error, setError] = useState<string | null>(null);

  const resetForm = useCallback(() => {
    setReason("");
    setError(null);
  }, []);

  const closeDialog = useCallback(() => {
    resetForm();
    onOpenChange(false);
  }, [onOpenChange, resetForm]);

  useEffect(() => {
    if (!open) return;

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

  if (!open) {
    return null;
  }

  async function handleConfirm() {
    const trimmedReason = reason.trim();
    if (!trimmedReason) {
    setError("Cancellation reason is required.");
      return;
    }

    setError(null);
    await onConfirm(trimmedReason);
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
        aria-labelledby="cancel-parcel-title"
        className={cn(
          "relative z-10 w-full max-w-lg overflow-hidden rounded-2xl border border-border/70",
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
                id="cancel-parcel-title"
                className="text-lg font-semibold leading-tight tracking-tight"
              >
                Cancel this parcel?
              </h2>
              <p className="text-sm leading-relaxed text-muted-foreground">
                This removes the parcel from the pre-load dispatch workflow and marks
                it as cancelled.
              </p>
            </div>
          </div>

          <div className="mt-5 flex items-center gap-3 rounded-xl border border-border/60 bg-muted/40 px-4 py-3">
            <PackageX className="size-5 shrink-0 text-muted-foreground" aria-hidden />
            <div className="min-w-0">
              <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
                Tracking number
              </p>
              <p className="truncate font-mono text-base font-semibold tabular-nums tracking-wide">
                {trackingNumber}
              </p>
            </div>
          </div>
        </div>

        <div className="space-y-3 px-6 py-5">
          <label htmlFor="cancel-reason" className="block text-sm font-medium">
            Cancellation reason
          </label>
          <textarea
            id="cancel-reason"
            value={reason}
            onChange={(event) => {
              setReason(event.target.value);
              if (error) {
                setError(null);
              }
            }}
            disabled={isPending}
            rows={4}
            className={cn(
              "min-h-28 w-full rounded-xl border border-input/90 bg-background px-3.5 py-2.5 text-sm shadow-sm transition-[color,box-shadow,border-color] outline-none",
              "hover:border-input focus-visible:border-ring focus-visible:ring-[3px] focus-visible:ring-ring/45",
              "disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50",
              error && "border-destructive focus-visible:ring-destructive/30",
            )}
            placeholder="Explain why this parcel should be cancelled."
            aria-invalid={error ? true : undefined}
          />
          {error ? <p className="text-sm text-destructive">{error}</p> : null}
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
            Keep parcel
          </Button>
          <Button
            type="button"
            variant="destructive"
            size="lg"
            disabled={isPending}
            className="w-full gap-2 sm:w-auto"
            onClick={() => void handleConfirm()}
          >
            {isPending ? (
              <>
                <Loader2 className="size-4 animate-spin shrink-0" aria-hidden />
                Cancelling...
              </>
            ) : (
              "Cancel parcel"
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
