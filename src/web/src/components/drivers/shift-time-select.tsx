"use client";

import { useMemo } from "react";

import { SelectDropdown } from "@/components/form/select-dropdown";
import {
  normalizeTimeToHms,
  optionsWithFallback,
  SHIFT_TIME_OPTIONS_15,
} from "@/lib/time/shift-time";

interface ShiftTimeSelectProps {
  id?: string;
  value: string;
  onChange: (valueHms: string) => void;
  disabled?: boolean;
  invalid?: boolean;
  /** Shown when value is empty before first selection. */
  placeholder?: string;
}

/**
 * Compact time picker for shift start/end (15‑minute steps, 24h labels, `HH:mm:ss` values).
 */
export function ShiftTimeSelect({
  id,
  value,
  onChange,
  disabled,
  invalid,
  placeholder = "Time",
}: ShiftTimeSelectProps) {
  const normalized = normalizeTimeToHms(value);

  const options = useMemo(
    () => optionsWithFallback(SHIFT_TIME_OPTIONS_15, normalized),
    [normalized],
  );

  return (
    <SelectDropdown
      id={id}
      options={options}
      value={normalized}
      onChange={onChange}
      placeholder={placeholder}
      disabled={disabled}
      invalid={invalid}
      className="w-23 shrink-0 sm:w-24"
      triggerClassName="h-9 min-h-9 min-w-0 w-full rounded-lg border-input/80 px-2.5 py-1.5 text-sm font-medium tabular-nums shadow-sm"
    />
  );
}
