/**
 * Hot Chocolate / GraphQL DateTime expects ISO-8601. Values from &lt;input type="date"&gt; are
 * `YYYY-MM-DD` only and often fail with "DateTime cannot parse StringValueNode".
 */
export function toGraphQLDateTimeFromDateInput(
  value: string | null | undefined,
): string | null | undefined {
  if (value == null || value === "") return null;
  const v = value.trim();
  if (/^\d{4}-\d{2}-\d{2}$/.test(v)) {
    return `${v}T12:00:00.000Z`;
  }
  return v;
}

/** For HTML date inputs from API ISO strings. */
export function toDateInputValue(isoOrEmpty: string | null | undefined): string {
  if (!isoOrEmpty?.trim()) return "";
  const s = isoOrEmpty.trim();
  if (/^\d{4}-\d{2}-\d{2}$/.test(s)) return s;
  const slice = s.slice(0, 10);
  return /^\d{4}-\d{2}-\d{2}$/.test(slice) ? slice : "";
}
