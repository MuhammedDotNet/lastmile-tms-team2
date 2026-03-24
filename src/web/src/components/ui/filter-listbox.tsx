"use client";

import { useEffect, useState, useRef } from "react";
import { createPortal } from "react-dom";
import { ChevronDown } from "lucide-react";

import { useFloatingDropdownPosition } from "@/lib/use-floating-dropdown-position";
import { cn } from "@/lib/utils";

export type FilterListboxOption<T> = { value: T | undefined; label: string };

const triggerBaseClass =
  "flex h-10 min-w-[180px] items-center justify-between rounded-xl border border-input/90 bg-background px-4 py-2 text-sm font-medium shadow-sm transition-[border-color,box-shadow,background-color] hover:border-input hover:bg-muted/40 focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-ring/45 dark:border-input dark:bg-input/25";

interface FilterListboxProps<T> {
  value: T | undefined;
  onChange: (value: T | undefined) => void;
  options: FilterListboxOption<T>[];
  className?: string;
}

export function FilterListbox<T>({
  value,
  onChange,
  options,
  className,
}: FilterListboxProps<T>) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);
  const pos = useFloatingDropdownPosition(open, triggerRef, { minWidth: 180 });

  const selectedLabel =
    options.find((o) => o.value === value)?.label ?? options[0]?.label ?? "";

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      const t = e.target as Node;
      if (
        triggerRef.current?.contains(t) ||
        panelRef.current?.contains(t)
      ) {
        return;
      }
      setOpen(false);
    }
    if (open) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [open]);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [open]);

  const panel =
    typeof document !== "undefined" &&
    open &&
    pos ? (
      <div
        ref={panelRef}
        role="listbox"
        style={{
          position: "fixed",
          top: pos.top,
          left: pos.left,
          width: pos.width,
          zIndex: 100,
        }}
        className="max-h-[min(22rem,calc(100vh-2rem))] overflow-y-auto rounded-xl border border-border bg-popover py-1.5 text-popover-foreground shadow-lg ring-1 ring-black/5 dark:ring-white/10"
      >
        {options.map((opt) => (
          <button
            key={String(opt.value ?? "__all__")}
            type="button"
            role="option"
            aria-selected={value === opt.value}
            onClick={() => {
              onChange(opt.value);
              setOpen(false);
            }}
            className={cn(
              "mx-1 block w-[calc(100%-0.5rem)] rounded-lg px-3 py-2.5 text-left text-sm transition-colors hover:bg-muted/80",
              value === opt.value &&
                "bg-muted font-medium text-foreground shadow-sm",
            )}
          >
            {opt.label}
          </button>
        ))}
      </div>
    ) : null;

  return (
    <div className={cn("relative", className)}>
      <button
        ref={triggerRef}
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={triggerBaseClass}
        aria-expanded={open}
        aria-haspopup="listbox"
      >
        <span>{selectedLabel}</span>
        <ChevronDown
          className={cn(
            "h-4 w-4 shrink-0 text-muted-foreground transition-transform",
            open && "rotate-180",
          )}
        />
      </button>

      {panel ? createPortal(panel, document.body) : null}
    </div>
  );
}
