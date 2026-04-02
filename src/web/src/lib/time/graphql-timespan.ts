import { normalizeTimeToHms } from "./shift-time";

/**
 * HotChocolate `TimeSpan` scalar expects ISO-8601 duration literals (e.g. `PT8H0M0S`),
 * not clock strings like `08:00:00`.
 */
export function toGraphQLTimeSpanFromHms(hms: string): string {
  const normalized = normalizeTimeToHms(hms);
  const [hh, mm, ss] = normalized.split(":").map((p) => Number.parseInt(p, 10));
  return `PT${hh}H${mm}M${ss}S`;
}

/**
 * Maps API `TimeSpan` strings (ISO duration or legacy clock-like strings) to `HH:mm:ss`
 * for shift dropdowns.
 */
export function timeSpanScalarToHms(value: string | null | undefined): string | null {
  if (value == null || value === "") {
    return null;
  }
  const v = value.trim();
  if (v.startsWith("P") || v.startsWith("-P")) {
    const rest = v.startsWith("-") ? v.slice(1) : v;
    const m =
      /^P(?:(\d+)D)?(?:T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+(?:\.\d+)?)S)?)?$/i.exec(
        rest,
      );
    if (!m) {
      return normalizeTimeToHms(v);
    }
    const days = Number.parseInt(m[1] ?? "0", 10);
    const hours = Number.parseInt(m[2] ?? "0", 10);
    const minutes = Number.parseInt(m[3] ?? "0", 10);
    const seconds = Math.floor(Number.parseFloat(m[4] ?? "0"));
    const totalHours = days * 24 + hours;
    return `${String(totalHours).padStart(2, "0")}:${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;
  }
  return normalizeTimeToHms(v);
}
