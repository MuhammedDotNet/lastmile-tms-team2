import { getSession } from "next-auth/react";

export function apiBaseUrl(): string {
  const url = process.env.NEXT_PUBLIC_API_URL;
  if (!url) {
    throw new Error("NEXT_PUBLIC_API_URL is not configured");
  }
  return url;
}

/**
 * Collects human-readable messages from `errors` whether the API returns:
 * - GraphQL / FluentValidation: `[{ message: "..." }]`
 * - ASP.NET model state: `{ "Field": ["msg"] }` or `{ "Field": "msg" }`
 */
function collectMessagesFromErrorsField(errors: unknown): string[] {
  if (errors == null) return [];

  if (Array.isArray(errors)) {
    return errors
      .map((item) => {
        if (item && typeof item === "object" && "message" in item) {
          return String((item as { message: unknown }).message ?? "").trim();
        }
        return "";
      })
      .filter(Boolean);
  }

  if (typeof errors === "object") {
    const out: string[] = [];
    for (const value of Object.values(errors as Record<string, unknown>)) {
      if (Array.isArray(value)) {
        for (const v of value) {
          if (typeof v === "string" && v.trim()) out.push(v.trim());
        }
      } else if (typeof value === "string" && value.trim()) {
        out.push(value.trim());
      }
    }
    return out;
  }

  return [];
}

/** Reads FluentValidation, GraphQL, RFC 7807 ProblemDetails, and ASP.NET validation bodies. */
export async function parseApiErrorMessage(res: Response): Promise<string> {
  let text: string;
  try {
    text = await res.text();
  } catch {
    return `Request failed (${res.status})`;
  }

  const trimmed = text.trim();
  if (!trimmed) {
    return `Request failed (${res.status})`;
  }

  try {
    const json = JSON.parse(trimmed) as Record<string, unknown>;

    const fromErrors = collectMessagesFromErrorsField(json.errors);
    if (fromErrors.length > 0) {
      return fromErrors.join(" ");
    }

    if (typeof json.detail === "string" && json.detail.trim()) {
      return json.detail.trim();
    }

    if (typeof json.title === "string" && json.title.trim()) {
      return json.title.trim();
    }

    if (typeof json.message === "string" && json.message.trim()) {
      return json.message.trim();
    }

    if (typeof json.error === "string" && json.error) {
      return json.error;
    }
  } catch {
    // not JSON — return raw body
  }

  return trimmed.length > 500 ? `${trimmed.slice(0, 500)}…` : trimmed;
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const session = await getSession();
  const headers = new Headers(init?.headers);

  if (!headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const token = session?.accessToken;
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  const res = await fetch(`${apiBaseUrl()}${path}`, {
    ...init,
    headers,
  });

  if (!res.ok) {
    const message = await parseApiErrorMessage(res);
    throw new Error(message);
  }

  return res.json() as Promise<T>;
}
