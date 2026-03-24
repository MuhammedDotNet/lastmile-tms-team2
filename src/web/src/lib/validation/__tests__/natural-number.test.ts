import { describe, expect, it } from "vitest";

import {
  formatNaturalIntForInput,
  parseNaturalIntInput,
} from "../natural-number";

describe("parseNaturalIntInput", () => {
  it("returns null for empty string", () => {
    expect(parseNaturalIntInput("", { min: 0 })).toBeNull();
  });

  it("strips non-digits and parses", () => {
    expect(parseNaturalIntInput("12x3", { min: 0 })).toBe(123);
  });

  it("clamps to min 0", () => {
    expect(parseNaturalIntInput("0", { min: 0 })).toBe(0);
  });

  it("respects max", () => {
    expect(parseNaturalIntInput("999", { min: 0, max: 50 })).toBe(50);
  });
});

describe("formatNaturalIntForInput", () => {
  it("formats non-negative integers", () => {
    expect(formatNaturalIntForInput(0)).toBe("0");
    expect(formatNaturalIntForInput(42)).toBe("42");
  });
});
