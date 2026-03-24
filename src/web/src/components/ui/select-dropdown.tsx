"use client";

import { useEffect, useState, useRef } from "react";
import { createPortal } from "react-dom";
import { ChevronDown } from "lucide-react";

import { useFloatingDropdownPosition } from "@/lib/use-floating-dropdown-position";
import { cn } from "@/lib/utils";

export interface SelectOption<T = string | number> {
  value: T;
  label: string;
}

interface SelectDropdownProps<T extends string | number> {
  options: SelectOption<T>[];
  value: T;
  onChange: (value: T) => void;
  placeholder?: string;
  className?: string;
  id?: string;
  disabled?: boolean;
  /** Validation error styling on trigger. */
  invalid?: boolean;
}

export function SelectDropdown<T extends string | number>({
  options,
  value,
  onChange,
  placeholder = "Select...",
  className = "",
  id,
  disabled = false,
  invalid = false,
}: SelectDropdownProps<T>) {
  const [open, setOpen] = useState(false);
  const triggerRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);
  const dropdownOpen = open && !disabled;
  const pos = useFloatingDropdownPosition(dropdownOpen, triggerRef, {
    minWidth: 140,
  });

  const selectedOption = options.find(
    (opt) => opt.value === value || String(opt.value) === String(value),
  );
  const displayLabel = selectedOption ? selectedOption.label : placeholder;

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
    if (dropdownOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [dropdownOpen]);

  useEffect(() => {
    if (!dropdownOpen) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") setOpen(false);
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [dropdownOpen]);

  const panel =
    typeof document !== "undefined" &&
    dropdownOpen &&
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
            key={String(opt.value)}
            type="button"
            role="option"
            aria-selected={value === opt.value}
            onClick={() => {
              onChange(opt.value);
              setOpen(false);
            }}
            className={cn(
              "mx-1 block w-[calc(100%-0.5rem)] rounded-lg px-3 py-2.5 text-left text-sm transition-colors",
              "hover:bg-muted/80",
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
        id={id}
        disabled={disabled}
        onClick={() => !disabled && setOpen((o) => !o)}
        className={cn(
          "flex h-10 w-full min-w-[140px] items-center justify-between rounded-xl border border-input/90 bg-background px-4 py-2 text-sm font-medium shadow-sm transition-[border-color,box-shadow,background-color]",
          "hover:border-input hover:bg-muted/40",
          "focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-ring/45",
          "dark:border-input dark:bg-input/25",
          invalid &&
            "border-destructive bg-destructive/[0.06] ring-1 ring-destructive/35 focus-visible:ring-destructive/40 dark:bg-destructive/10 dark:ring-destructive/45",
          disabled && "pointer-events-none opacity-50",
        )}
        data-invalid={invalid || undefined}
        aria-expanded={dropdownOpen}
        aria-haspopup="listbox"
        aria-labelledby={id ? `${id}-label` : undefined}
      >
        <span className={!selectedOption ? "text-muted-foreground" : ""}>
          {displayLabel}
        </span>
        <ChevronDown
          className={cn(
            "h-4 w-4 shrink-0 text-muted-foreground transition-transform duration-200",
            open && "rotate-180",
          )}
        />
      </button>

      {panel ? createPortal(panel, document.body) : null}
    </div>
  );
}
