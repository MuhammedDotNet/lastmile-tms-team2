/**
 * Unwraps thrown values from fetch, GraphQL, React Query, and plain objects
 * so mutation toasts show the real server message instead of a generic fallback.
 */
export function getErrorMessage(error: unknown): string {
  const fallback = "Something went wrong. Please try again.";

  if (error == null) return fallback;

  if (typeof error === "string") {
    const s = error.trim();
    return s || fallback;
  }

  if (typeof error === "number" || typeof error === "boolean") {
    return String(error);
  }

  if (error instanceof Error) {
    const msg = error.message?.trim();
    const cause = (error as Error & { cause?: unknown }).cause;
    if (cause !== undefined && cause !== null) {
      const causeMsg = getErrorMessage(cause);
      if (msg && causeMsg && causeMsg !== fallback) {
        return `${msg}: ${causeMsg}`;
      }
      if (!msg && causeMsg) return causeMsg;
    }
    if (msg) return msg;
  }

  if (typeof error === "object") {
    const o = error as Record<string, unknown>;

    if (typeof o.message === "string" && o.message.trim()) {
      return o.message.trim();
    }

    const nested = o.response ?? o.body;
    if (nested && typeof nested === "object") {
      const data = (nested as { data?: unknown }).data ?? nested;
      if (data && typeof data === "object") {
        const d = data as Record<string, unknown>;
        if (typeof d.message === "string" && d.message.trim()) {
          return d.message.trim();
        }
        if (Array.isArray(d.errors) && d.errors.length > 0) {
          const first = d.errors[0];
          if (
            first &&
            typeof first === "object" &&
            "message" in first &&
            typeof (first as { message: unknown }).message === "string"
          ) {
            return String((first as { message: string }).message).trim();
          }
        }
      }
    }
  }

  return fallback;
}
