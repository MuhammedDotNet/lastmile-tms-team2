import type { SelectOption } from "@/types/forms";

/** 12-hour label for shift dropdowns (values stay `HH:mm:ss` for the API). */
export function formatShiftTimeLabel(hms: string): string {
  const parts = hms.split(":");
  const h = Number.parseInt(parts[0] ?? "0", 10);
  const m = Number.parseInt(parts[1] ?? "0", 10);
  const period = h >= 12 ? "PM" : "AM";
  const hour12 = h % 12 || 12;
  return `${hour12}:${String(m).padStart(2, "0")} ${period}`;
}

/** Normalizes stored shift strings to `HH:mm:ss` for option matching. */
export function normalizeTimeToHms(value: string): string {
  const v = value.trim();
  if (!v) {
    return "08:00:00";
  }
  const noMs = v.split(".")[0] ?? v;
  const parts = noMs.split(":");
  if (parts.length === 2) {
    return `${parts[0]!.padStart(2, "0")}:${parts[1]!.padStart(2, "0")}:00`;
  }
  if (parts.length >= 3) {
    return `${parts[0]!.padStart(2, "0")}:${parts[1]!.padStart(2, "0")}:${parts[2]!.padStart(2, "0").slice(0, 2)}`;
  }
  return "08:00:00";
}

function buildShiftTimeOptions(stepMinutes: number): SelectOption<string>[] {
  const out: SelectOption<string>[] = [];
  for (let h = 0; h < 24; h++) {
    for (let m = 0; m < 60; m += stepMinutes) {
      const hh = String(h).padStart(2, "0");
      const mm = String(m).padStart(2, "0");
      const value = `${hh}:${mm}:00`;
      out.push({ value, label: formatShiftTimeLabel(value) });
    }
  }
  return out;
}

/** 15-minute slots, 00:00–23:45 (values `HH:mm:ss`). */
export const SHIFT_TIME_OPTIONS_15: SelectOption<string>[] =
  buildShiftTimeOptions(15);

export function optionsWithFallback(
  base: SelectOption<string>[],
  valueHms: string,
): SelectOption<string>[] {
  if (base.some((o) => o.value === valueHms)) {
    return base;
  }
  return [{ value: valueHms, label: formatShiftTimeLabel(valueHms) }, ...base];
}
