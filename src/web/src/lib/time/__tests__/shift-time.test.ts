import { describe, expect, it } from "vitest";

import {
  normalizeTimeToHms,
  optionsWithFallback,
  SHIFT_TIME_OPTIONS_15,
} from "../shift-time";

describe("normalizeTimeToHms", () => {
  it("pads two-part times to HH:mm:ss", () => {
    expect(normalizeTimeToHms("8:30")).toBe("08:30:00");
    expect(normalizeTimeToHms("08:30")).toBe("08:30:00");
  });

  it("keeps three-part times", () => {
    expect(normalizeTimeToHms("08:30:00")).toBe("08:30:00");
  });

  it("defaults empty to 08:00:00", () => {
    expect(normalizeTimeToHms("")).toBe("08:00:00");
  });
});

describe("SHIFT_TIME_OPTIONS_15", () => {
  it("has 15-minute steps for 24h", () => {
    expect(SHIFT_TIME_OPTIONS_15.length).toBe(96);
    expect(SHIFT_TIME_OPTIONS_15[0]).toEqual({
      value: "00:00:00",
      label: "12:00 AM",
    });
  });
});

describe("optionsWithFallback", () => {
  it("prepends unknown time", () => {
    const opts = optionsWithFallback(SHIFT_TIME_OPTIONS_15, "07:22:00");
    expect(opts[0]).toEqual({ value: "07:22:00", label: "7:22 AM" });
  });
});
