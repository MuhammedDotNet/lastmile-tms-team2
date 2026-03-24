import { describe, expect, it } from "vitest";

import { guidString } from "../guid-string";

const msg = "bad";

describe("guidString", () => {
  it("accepts seeded depot and parcel style GUIDs", () => {
    const s = guidString(msg);
    expect(s.safeParse("00000000-0000-0000-0000-000000000001").success).toBe(
      true,
    );
    expect(s.safeParse("40000000-0000-0000-0000-000000000001").success).toBe(
      true,
    );
    expect(s.safeParse("550e8400-e29b-41d4-a716-446655440000").success).toBe(
      true,
    );
  });

  it("rejects empty and non-GUID strings", () => {
    const s = guidString(msg);
    expect(s.safeParse("").success).toBe(false);
    expect(s.safeParse("not-a-uuid").success).toBe(false);
  });
});
