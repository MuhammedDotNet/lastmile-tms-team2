"use client";

import { useCallback } from "react";
import { Clock } from "lucide-react";

import { ShiftTimeSelect } from "@/components/drivers/shift-time-select";
import { DAY_OF_WEEK_LABELS } from "@/lib/labels/drivers";
import { cn } from "@/lib/utils";
import type { DayOfWeek } from "@/graphql/generated";

interface AvailabilityEntry {
  id?: string | null;
  dayOfWeek: DayOfWeek;
  shiftStart: string;
  shiftEnd: string;
  isAvailable: boolean;
}

interface DriverAvailabilityFormProps {
  value: AvailabilityEntry[];
  onChange: (value: AvailabilityEntry[]) => void;
  errors?: Record<string, string>;
}

export function DriverAvailabilityForm({
  value,
  onChange,
  errors,
}: DriverAvailabilityFormProps) {
  const handleToggle = useCallback(
    (dayIndex: number) => {
      const newValue = [...value];
      newValue[dayIndex] = {
        ...newValue[dayIndex],
        isAvailable: !newValue[dayIndex].isAvailable,
      };
      onChange(newValue);
    },
    [value, onChange],
  );

  const handleShiftChange = useCallback(
    (dayIndex: number, field: "shiftStart" | "shiftEnd", val: string) => {
      const newValue = [...value];
      newValue[dayIndex] = {
        ...newValue[dayIndex],
        [field]: val,
      };
      onChange(newValue);
    },
    [value, onChange],
  );

  return (
    <div className="space-y-2.5">
      {errors?.availabilitySchedule && (
        <p className="text-sm text-destructive" role="alert">
          {errors.availabilitySchedule}
        </p>
      )}
      {value.map((entry, index) => (
        <div
          key={entry.dayOfWeek}
          className={cn(
            "flex flex-col gap-3 rounded-xl border border-border/80 bg-card/40 p-3.5 sm:flex-row sm:items-center sm:justify-between sm:gap-4",
            "shadow-sm ring-1 ring-black/3 dark:ring-white/6",
          )}
        >
          <div className="flex min-w-0 items-center gap-3 sm:w-44">
            <label className="relative inline-flex cursor-pointer items-center">
              <input
                type="checkbox"
                checked={entry.isAvailable}
                onChange={() => handleToggle(index)}
                className="peer sr-only"
              />
              <span
                className={cn(
                  "relative h-6 w-11 shrink-0 rounded-full bg-muted-foreground/25 transition-colors",
                  "peer-checked:bg-primary",
                  "after:absolute after:inset-s-0.5 after:top-0.5 after:h-5 after:w-5 after:rounded-full after:bg-white after:shadow-sm after:transition-transform after:content-['']",
                  "peer-checked:after:translate-x-5",
                )}
                aria-hidden
              />
            </label>
            <span
              className={cn(
                "truncate text-sm font-medium",
                entry.isAvailable
                  ? "text-foreground"
                  : "text-muted-foreground line-through decoration-muted-foreground/50",
              )}
            >
              {DAY_OF_WEEK_LABELS[entry.dayOfWeek]}
            </span>
          </div>

          {entry.isAvailable && (
            <div className="flex flex-wrap items-center gap-x-2 gap-y-2 sm:justify-end">
              <div
                className="flex flex-wrap items-center gap-2 rounded-lg bg-muted/50 px-2.5 py-1.5 ring-1 ring-border/60"
                role="group"
                aria-label={`${DAY_OF_WEEK_LABELS[entry.dayOfWeek]} shift`}
              >
                <Clock
                  className="size-3.5 shrink-0 text-muted-foreground"
                  strokeWidth={2}
                  aria-hidden
                />
                <span className="text-[11px] font-medium uppercase tracking-wide text-muted-foreground">
                  From
                </span>
                <ShiftTimeSelect
                  id={`shift-start-${entry.dayOfWeek}`}
                  value={entry.shiftStart}
                  onChange={(v) => handleShiftChange(index, "shiftStart", v)}
                  placeholder="Start"
                />
                <span
                  className="px-0.5 text-muted-foreground/70 tabular-nums"
                  aria-hidden
                >
                  –
                </span>
                <span className="text-[11px] font-medium uppercase tracking-wide text-muted-foreground">
                  To
                </span>
                <ShiftTimeSelect
                  id={`shift-end-${entry.dayOfWeek}`}
                  value={entry.shiftEnd}
                  onChange={(v) => handleShiftChange(index, "shiftEnd", v)}
                  placeholder="End"
                />
              </div>
            </div>
          )}

          {!entry.isAvailable && (
            <span className="text-sm text-muted-foreground sm:text-right">
              Day off
            </span>
          )}
        </div>
      ))}
    </div>
  );
}
