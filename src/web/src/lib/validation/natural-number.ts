/**
 * Parses integer input: digits only, optional bounds.
 * Empty string returns `null` (caller may coerce to min for controlled UX).
 */
export function parseNaturalIntInput(
  raw: string,
  options: { min: number; max?: number },
): number | null {
  const digits = raw.replace(/\D/g, "");
  if (digits === "") return null;
  let n = parseInt(digits, 10);
  if (Number.isNaN(n)) return null;
  n = Math.max(options.min, n);
  if (options.max !== undefined) {
    n = Math.min(options.max, n);
  }
  return n;
}

/** Strip leading zeros for display (keep single "0"). */
export function formatNaturalIntForInput(value: number): string {
  if (!Number.isFinite(value) || value < 0) return "0";
  return String(Math.trunc(value));
}
