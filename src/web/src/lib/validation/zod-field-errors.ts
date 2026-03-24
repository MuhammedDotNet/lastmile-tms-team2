import type { ZodError } from "zod";

/** First message per dot-path key (e.g. `parcelCapacity`). */
export function zodErrorToFieldMap(error: ZodError): Record<string, string> {
  const map: Record<string, string> = {};
  for (const issue of error.issues) {
    const key = issue.path.length ? issue.path.join(".") : "_root";
    if (map[key] === undefined) {
      map[key] = issue.message;
    }
  }
  return map;
}
