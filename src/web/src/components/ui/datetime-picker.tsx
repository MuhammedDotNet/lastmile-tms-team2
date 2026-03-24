"use client";

import { useEffect, useState, useRef } from "react";
import { createPortal } from "react-dom";
import { Calendar, ChevronLeft, ChevronRight } from "lucide-react";

import { useFloatingDropdownPosition } from "@/lib/use-floating-dropdown-position";
import { cn } from "@/lib/utils";

const WEEKDAYS = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const MONTHS = [
  "January", "February", "March", "April", "May", "June",
  "July", "August", "September", "October", "November", "December",
];

function formatDateTime(value: string): string {
  if (!value) return "Select date & time";
  const d = new Date(value);
  if (isNaN(d.getTime())) return "Select date & time";
  const dateStr = d.toLocaleDateString(undefined, { dateStyle: "medium" });
  const h = d.getHours();
  const hour12 = h === 0 ? 12 : h > 12 ? h - 12 : h;
  const ampm = h >= 12 ? "PM" : "AM";
  const minStr = String(d.getMinutes()).padStart(2, "0");
  return `${dateStr}, ${hour12}:${minStr} ${ampm}`;
}

function toInputFormat(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  const h = String(date.getHours()).padStart(2, "0");
  const min = String(date.getMinutes()).padStart(2, "0");
  return `${y}-${m}-${d}T${h}:${min}`;
}

interface DateTimePickerProps {
  value: string;
  onChange: (value: string) => void;
  className?: string;
  invalid?: boolean;
}

export function DateTimePicker({
  value,
  onChange,
  className = "",
  invalid = false,
}: DateTimePickerProps) {
  const [open, setOpen] = useState(false);
  const [viewDate, setViewDate] = useState(() => {
    const d = value ? new Date(value) : new Date();
    return new Date(d.getFullYear(), d.getMonth(), 1);
  });
  const to24h = (h12: number, pm: boolean) =>
    pm ? (h12 === 12 ? 12 : h12 + 12) : (h12 === 12 ? 0 : h12);

  const [hour12, setHour12] = useState(() => {
    const d = value ? new Date(value) : new Date();
    const h = d.getHours();
    return h === 0 ? 12 : h > 12 ? h - 12 : h;
  });
  const [minutes, setMinutes] = useState(() => {
    const d = value ? new Date(value) : new Date();
    return d.getMinutes();
  });
  const [isPM, setIsPM] = useState(() => {
    const d = value ? new Date(value) : new Date();
    return d.getHours() >= 12;
  });
  const triggerRef = useRef<HTMLButtonElement>(null);
  const panelRef = useRef<HTMLDivElement>(null);
  const pos = useFloatingDropdownPosition(open, triggerRef, { minWidth: 288 });

  const selectedDate = value ? new Date(value) : null;

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

  const hours24 = to24h(hour12, isPM);

  const handleSelectDay = (day: number) => {
    const d = new Date(
      viewDate.getFullYear(),
      viewDate.getMonth(),
      day,
      hours24,
      minutes,
    );
    onChange(toInputFormat(d));
  };

  const handleTimeChange = () => {
    const day = selectedDate?.getDate() ?? new Date().getDate();
    const d = new Date(
      viewDate.getFullYear(),
      viewDate.getMonth(),
      day,
      to24h(hour12, isPM),
      minutes,
    );
    onChange(toInputFormat(d));
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

  const panel =
    typeof document !== "undefined" &&
    open &&
    pos ? (
      <div
        ref={panelRef}
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

        <div className="mb-4 grid grid-cols-7 gap-1">
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
            )
          )}
        </div>

        <div className="flex flex-wrap items-center gap-3 border-t border-border/60 pt-3">
          <span className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            Time
          </span>
          <div className="flex flex-1 flex-wrap items-center justify-end gap-2 sm:justify-start">
            <input
              type="number"
              min={1}
              max={12}
              value={hour12}
              onChange={(e) => {
                const v = Math.min(12, Math.max(1, Number(e.target.value) || 1));
                setHour12(v);
                setTimeout(handleTimeChange, 0);
              }}
              onBlur={handleTimeChange}
              className="w-12 rounded-lg border border-input bg-background px-2 py-1.5 text-center text-sm tabular-nums text-foreground shadow-sm dark:bg-input/30"
            />
            <span className="text-muted-foreground">:</span>
            <input
              type="text"
              inputMode="numeric"
              maxLength={2}
              value={String(minutes).padStart(2, "0")}
              onChange={(e) => {
                const raw = e.target.value.replace(/\D/g, "");
                const v =
                  raw === ""
                    ? 0
                    : Math.min(59, Math.max(0, parseInt(raw, 10) || 0));
                setMinutes(v);
                setTimeout(handleTimeChange, 0);
              }}
              onBlur={handleTimeChange}
              className="w-12 rounded-lg border border-input bg-background px-2 py-1.5 text-center text-sm tabular-nums text-foreground shadow-sm dark:bg-input/30"
            />
            <button
              type="button"
              onClick={() => {
                setIsPM((p) => !p);
                setTimeout(handleTimeChange, 0);
              }}
              className="cursor-pointer rounded-lg border border-input bg-muted/60 px-2.5 py-1.5 text-sm font-semibold tabular-nums text-foreground transition-colors hover:bg-muted dark:bg-input/40"
            >
              {isPM ? "PM" : "AM"}
            </button>
          </div>
        </div>
      </div>
    ) : null;

  return (
    <div className={cn("relative", className)}>
      <button
        ref={triggerRef}
        type="button"
        onClick={() => setOpen((o) => !o)}
        className={cn(
          "flex h-10 w-full cursor-pointer items-center justify-between rounded-xl border border-input/90 bg-background px-4 py-2 text-sm shadow-sm transition-[border-color,box-shadow,background-color] hover:border-input hover:bg-muted/40 focus-visible:outline-none focus-visible:ring-[3px] focus-visible:ring-ring/45 dark:border-input dark:bg-input/25",
          invalid &&
            "border-destructive bg-destructive/[0.06] ring-1 ring-destructive/35 focus-visible:ring-destructive/40 dark:bg-destructive/10 dark:ring-destructive/45",
        )}
        aria-expanded={open}
        aria-haspopup="dialog"
        aria-label="Choose date and time"
      >
        <span className="flex min-w-0 items-center gap-2">
          <Calendar className="h-4 w-4 shrink-0 text-muted-foreground" />
          <span className="truncate text-left">{formatDateTime(value)}</span>
        </span>
      </button>

      {panel ? createPortal(panel, document.body) : null}
    </div>
  );
}
