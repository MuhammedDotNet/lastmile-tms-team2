import { describe, expect, it } from "vitest";
import { sortByDayOfWeek } from "../drivers";

describe("sortByDayOfWeek", () => {
  it("orders Monday through Sunday regardless of input order", () => {
    const rows = [
      { dayOfWeek: "SUNDAY" as const, k: 1 },
      { dayOfWeek: "MONDAY" as const, k: 2 },
      { dayOfWeek: "SATURDAY" as const, k: 3 },
      { dayOfWeek: "WEDNESDAY" as const, k: 4 },
    ];
    expect(sortByDayOfWeek(rows).map((r) => r.dayOfWeek)).toEqual([
      "MONDAY",
      "WEDNESDAY",
      "SATURDAY",
      "SUNDAY",
    ]);
  });
});
