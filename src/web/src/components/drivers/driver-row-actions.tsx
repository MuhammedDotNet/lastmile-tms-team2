"use client";

import Link from "next/link";
import { Eye, Pencil, Trash2 } from "lucide-react";
import { Tooltip } from "radix-ui";

import { buttonVariants } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type DriverRowActionsProps = {
  driverId: string;
  displayName: string;
  onDeleteAction: () => void;
  deleteDisabled?: boolean;
};

const iconBtn = cn(
  buttonVariants({ variant: "ghost", size: "icon-sm" }),
  "text-muted-foreground hover:bg-muted hover:text-foreground shrink-0"
);

const deleteBtn = cn(
  buttonVariants({ variant: "ghost", size: "icon-sm" }),
  "shrink-0 text-destructive hover:bg-destructive/10 hover:text-destructive"
);

export function DriverRowActions({
  driverId,
  displayName,
  onDeleteAction,
  deleteDisabled,
}: DriverRowActionsProps) {
  return (
    <div className="flex items-center justify-end gap-0.5">
      <Tooltip.Root>
        <Tooltip.Trigger asChild>
          <Link
            href={`/drivers/${driverId}`}
            className={iconBtn}
            aria-label={`View ${displayName}`}
          >
            <Eye className="size-3.5" strokeWidth={2} aria-hidden />
          </Link>
        </Tooltip.Trigger>
        <Tooltip.Portal>
          <Tooltip.Content
            side="top"
            sideOffset={4}
            className="z-50 rounded-md border border-border bg-popover px-2 py-1 text-xs text-popover-foreground shadow-md"
          >
            View
            <Tooltip.Arrow className="fill-popover" />
          </Tooltip.Content>
        </Tooltip.Portal>
      </Tooltip.Root>

      <Tooltip.Root>
        <Tooltip.Trigger asChild>
          <Link
            href={`/drivers/${driverId}/edit`}
            className={iconBtn}
            aria-label={`Edit ${displayName}`}
          >
            <Pencil className="size-3.5" strokeWidth={2} aria-hidden />
          </Link>
        </Tooltip.Trigger>
        <Tooltip.Portal>
          <Tooltip.Content
            side="top"
            sideOffset={4}
            className="z-50 rounded-md border border-border bg-popover px-2 py-1 text-xs text-popover-foreground shadow-md"
          >
            Edit
            <Tooltip.Arrow className="fill-popover" />
          </Tooltip.Content>
        </Tooltip.Portal>
      </Tooltip.Root>

      <Tooltip.Root>
        <Tooltip.Trigger asChild>
          <button
            type="button"
            className={deleteBtn}
            disabled={deleteDisabled}
            aria-label={`Delete ${displayName}`}
            onClick={onDeleteAction}
          >
            <Trash2 className="size-3.5" strokeWidth={2} aria-hidden />
          </button>
        </Tooltip.Trigger>
        <Tooltip.Portal>
          <Tooltip.Content
            side="top"
            sideOffset={4}
            className="z-50 rounded-md border border-border bg-popover px-2 py-1 text-xs text-popover-foreground shadow-md"
          >
            Delete
            <Tooltip.Arrow className="fill-popover" />
          </Tooltip.Content>
        </Tooltip.Portal>
      </Tooltip.Root>
    </div>
  );
}
