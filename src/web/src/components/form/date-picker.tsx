"use client";

import { useEffect, useId, useRef, useState, startTransition } from "react";
import { createPortal } from "react-dom";
import { Calendar, ChevronLeft, ChevronRight } from "lucide-react";

import { useFloatingDropdownPosition } from "@/hooks/use-floating-dropdown-position";
import { cn } from "@/lib/utils";

const WEEKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const MONTHS = [
  "January",
  "February",
  "March",
  "April",
  "May",
  "June",
  "July",
  "August",
  "September",
  "October",
  "November",
  "December",
];

/** Local calendar date from `YYYY-MM-DD` (no UTC shift). */
export function parseYmdLocal(ymd: string): Date | null {
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(ymd.trim());
  if (!m) return null;
  const y = Number(m[1]);
  const mo = Number(m[2]) - 1;
  const day = Number(m[3]);
  const d = new Date(y, mo, day);
  if (
    d.getFullYear() !== y ||
    d.getMonth() !== mo ||
    d.getDate() !== day
  ) {
    return null;
  }
  return d;
}

export function formatYmdLocal(d: Date): string {
  const y = d.getFullYear();
  const mo = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${mo}-${day}`;
}

function formatDateLabel(value: string): string {
  if (!value) return "Select date";
  const d = parseYmdLocal(value);
  if (!d) return "Select date";
  return d.toLocaleDateString(undefined, { dateStyle: "medium" });
}

export interface DatePickerProps {
  value: string;
  onChange: (value: string) => void;
  className?: string;
  invalid?: boolean;
  placeholder?: string;
  /** Shown in the trigger when empty (defaults to "Select date"). */
  emptyLabel?: string;
  id?: string;
  "aria-invalid"?: boolean | "true" | "false";
  "aria-labelledby"?: string;
}

export function DatePicker({
  value,
  onChange,
  className = "",
  invalid = false,
  emptyLabel,
  id,
  "aria-invalid": ariaInvalid,
  "aria-labelledby": ariaLabelledBy,
}: DatePickerProps) {
  const emptyText = emptyLabel ?? "Select date";
  const calendarPanelId = useId();
  const [open, setOpen] = useState(false);
  const [viewDate, setViewDate] = useState(() => {
    const d = value ? parseYmdLocal(value) : new Date();
    const base = d ?? new Date();
    return new Date(base.getFullYear(), base.getMonth(), 1);
  });

  const triggerRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);
  const pos = useFloatingDropdownPosition(open, triggerRef, { minWidth: 288 });

  useEffect(() => {
    if (!value) return;
    const d = parseYmdLocal(value);
    if (!d) return;
    const monthStart = new Date(d.getFullYear(), d.getMonth(), 1);
    startTransition(() => {
      setViewDate(monthStart);
    });
  }, [value]);

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

  const daysInMonth = new Date(
    viewDate.getFullYear(),
    viewDate.getMonth() + 1,
    0,
  ).getDate();
  const firstDay = new Date(
    viewDate.getFullYear(),
    viewDate.getMonth(),
    1,
  ).getDay();
  const days: (number | null)[] = [];
  for (let i = 0; i < firstDay; i++) days.push(null);
  for (let i = 1; i <= daysInMonth; i++) days.push(i);

  const selectedDate = value ? parseYmdLocal(value) : null;

  const handleSelectDay = (day: number) => {
    const d = new Date(viewDate.getFullYear(), viewDate.getMonth(), day);
    onChange(formatYmdLocal(d));
    setOpen(false);
  };

  const prevMonth = () => {
    setViewDate(new Date(viewDate.getFullYear(), viewDate.getMonth() - 1));
  };

  const nextMonth = () => {
    setViewDate(new Date(viewDate.getFullYear(), viewDate.getMonth() + 1));
  };

  const isSelected = (day: number): boolean =>
    !!(
      selectedDate &&
      selectedDate.getDate() === day &&
      selectedDate.getMonth() === viewDate.getMonth() &&
      selectedDate.getFullYear() === viewDate.getFullYear()
    );

  const isToday = (day: number) => {
    const today = new Date();
    return (
      today.getDate() === day &&
      today.getMonth() === viewDate.getMonth() &&
      today.getFullYear() === viewDate.getFullYear()
    );
  };

  const displayLabel = value ? formatDateLabel(value) : emptyText;

  const panel =
    typeof document !== "undefined" &&
    open &&
    pos ? (
      <div
        ref={panelRef}
        id={calendarPanelId}
        role="dialog"
        aria-label="Choose date"
        style={{
          position: "fixed",
          top: pos.top,
          left: pos.left,
          width: Math.max(pos.width, 288),
          zIndex: 100,
        }}
        className="rounded-xl border border-border bg-popover p-4 text-popover-foreground shadow-lg ring-1 ring-black/5 dark:ring-white/10"
      >
        <div className="mb-4 flex items-center justify-between">
          <button
            type="button"
            onClick={prevMonth}
            className="cursor-pointer rounded-lg p-1.5 text-foreground transition-colors hover:bg-muted"
            aria-label="Previous month"
          >
            <ChevronLeft className="h-5 w-5" />
          </button>
          <span className="text-sm font-semibold text-foreground">
            {MONTHS[viewDate.getMonth()]} {viewDate.getFullYear()}
          </span>
          <button
            type="button"
            onClick={nextMonth}
            className="cursor-pointer rounded-lg p-1.5 text-foreground transition-colors hover:bg-muted"
            aria-label="Next month"
          >
            <ChevronRight className="h-5 w-5" />
          </button>
        </div>

        <div className="mb-3 grid grid-cols-7 gap-1">
          {WEEKDAYS.map((wd) => (
            <div
              key={wd}
              className="py-1 text-center text-[11px] font-semibold uppercase tracking-wide text-muted-foreground"
            >
              {wd}
            </div>
          ))}
          {days.map((day, i) =>
            day === null ? (
              <div key={`empty-${i}`} />
            ) : (
              <button
                key={day}
                type="button"
                onClick={() => handleSelectDay(day)}
                className={cn(
                  "h-9 w-9 cursor-pointer rounded-lg text-sm font-medium transition-colors",
                  isSelected(day)
                    ? "bg-primary text-primary-foreground shadow-sm hover:bg-primary/90"
                    : isToday(day)
                      ? "border border-primary/40 bg-muted/80 text-foreground hover:bg-muted"
                      : "bg-transparent text-foreground hover:bg-muted",
                )}
              >
                {day}
              </button>
            ),
          )}
        </div>

        {value ? (
          <div className="border-t border-border/60 pt-3">
            <button
              type="button"
              onClick={() => {
                onChange("");
                setOpen(false);
              }}
              className="text-sm font-medium text-muted-foreground underline-offset-4 transition-colors hover:text-foreground hover:underline"
            >
              Clear date
            </button>
          </div>
        ) : null}
      </div>
    ) : null;

  return (
    <div className={cn("relative", className)}>
      <button
        ref={triggerRef}
        id={id}
        type="button"
        role="combobox"
        aria-controls={calendarPanelId}
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
        className={cn(
          "flex h-10 w-full cursor-pointer items-center justify-between rounded-xl border border-input/90 bg-background px-4 py-2 text-sm shadow-sm transition-[border-color,box-shadow,background-color] hover:border-input hover:bg-muted/40 focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-ring/45 dark:border-input dark:bg-input/25",
          invalid &&
            "border-destructive bg-destructive/6 ring-1 ring-destructive/35 focus-visible:ring-destructive/40 dark:bg-destructive/10 dark:ring-destructive/45",
        )}
        aria-haspopup="dialog"
        aria-label="Choose date"
        aria-invalid={ariaInvalid}
        aria-labelledby={ariaLabelledBy}
      >
        <span className="flex min-w-0 items-center gap-2">
          <Calendar className="h-4 w-4 shrink-0 text-muted-foreground" />
          <span
            className={cn(
              "truncate text-left",
              !value && "text-muted-foreground",
            )}
          >
            {displayLabel}
          </span>
        </span>
      </button>

      {panel ? createPortal(panel, document.body) : null}
    </div>
  );
}
