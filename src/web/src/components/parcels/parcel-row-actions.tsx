"use client";

import Link from "next/link";
import { Ban, Eye, Pencil } from "lucide-react";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type ParcelRowActionsProps = {
  parcelId: string;
  trackingNumber: string;
  onCancel: () => void;
  editDisabled?: boolean;
  cancelDisabled?: boolean;
};

const iconButtonClass = cn(
  buttonVariants({ variant: "ghost", size: "icon-sm" }),
  "shrink-0 text-muted-foreground hover:bg-muted hover:text-foreground",
);

const destructiveIconButtonClass = cn(
  buttonVariants({ variant: "ghost", size: "icon-sm" }),
  "shrink-0 text-destructive hover:bg-destructive/10 hover:text-destructive",
);

export function ParcelRowActions({
  parcelId,
  trackingNumber,
  onCancel,
  editDisabled = false,
  cancelDisabled = false,
}: ParcelRowActionsProps) {
  return (
    <div className="flex items-center justify-end gap-0.5">
      <Link
        href={`/parcels/${parcelId}`}
        className={iconButtonClass}
        aria-label={`View ${trackingNumber}`}
        title="View"
      >
        <Eye className="size-3.5" strokeWidth={2} aria-hidden />
      </Link>

      <Link
        href={`/parcels/${parcelId}/edit`}
        className={cn(iconButtonClass, editDisabled && "pointer-events-none opacity-50")}
        aria-label={`Edit ${trackingNumber}`}
        aria-disabled={editDisabled}
        tabIndex={editDisabled ? -1 : undefined}
        title="Edit"
      >
        <Pencil className="size-3.5" strokeWidth={2} aria-hidden />
      </Link>

      <button
        type="button"
        className={destructiveIconButtonClass}
        disabled={cancelDisabled}
        aria-label={`Cancel ${trackingNumber}`}
        title="Cancel"
        onClick={onCancel}
      >
        <Ban className="size-3.5" strokeWidth={2} aria-hidden />
      </button>
    </div>
  );
}
