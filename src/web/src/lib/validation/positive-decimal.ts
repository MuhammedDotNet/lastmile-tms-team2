/** Keeps digits and at most one decimal point (typing-friendly). */
export function sanitizePositiveDecimalInput(raw: string): string {
  const s = raw.replace(/[^\d.]/g, "");
  const dot = s.indexOf(".");
  if (dot === -1) return s;
  return s.slice(0, dot + 1) + s.slice(dot + 1).replace(/\./g, "");
}

export function parsePositiveDecimalInput(sanitized: string): number | null {
  if (sanitized === "" || sanitized === ".") return null;
  const n = parseFloat(sanitized);
  if (Number.isNaN(n) || !Number.isFinite(n)) return null;
  return n;
}
