import { describe, expect, it } from "vitest";
import { formatYmdLocal, parseYmdLocal } from "../date-picker";

describe("parseYmdLocal / formatYmdLocal", () => {
  it("round-trips a calendar date without UTC drift", () => {
    const d = new Date(2028, 5, 15);
    const ymd = formatYmdLocal(d);
    expect(ymd).toBe("2028-06-15");
    const back = parseYmdLocal(ymd);
    expect(back?.getFullYear()).toBe(2028);
    expect(back?.getMonth()).toBe(5);
    expect(back?.getDate()).toBe(15);
  });

  it("rejects invalid calendar days", () => {
    expect(parseYmdLocal("2028-02-30")).toBeNull();
  });
});
