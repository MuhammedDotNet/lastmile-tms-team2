import { describe, expect, it } from "vitest";
import {
  timeSpanScalarToHms,
  toGraphQLTimeSpanFromHms,
} from "../graphql-timespan";

describe("toGraphQLTimeSpanFromHms", () => {
  it("emits ISO-8601 duration for morning shift", () => {
    expect(toGraphQLTimeSpanFromHms("08:00:00")).toBe("PT8H0M0S");
  });

  it("normalizes two-segment time", () => {
    expect(toGraphQLTimeSpanFromHms("9:30")).toBe("PT9H30M0S");
  });

  it("handles afternoon", () => {
    expect(toGraphQLTimeSpanFromHms("17:00:00")).toBe("PT17H0M0S");
  });
});

describe("timeSpanScalarToHms", () => {
  it("parses ISO duration from API", () => {
    expect(timeSpanScalarToHms("PT8H0M0S")).toBe("08:00:00");
    expect(timeSpanScalarToHms("PT17H30M0S")).toBe("17:30:00");
  });

  it("parses short ISO form", () => {
    expect(timeSpanScalarToHms("PT8H")).toBe("08:00:00");
  });

  it("passes through clock-like strings", () => {
    expect(timeSpanScalarToHms("08:00:00")).toBe("08:00:00");
  });

  it("returns null for empty", () => {
    expect(timeSpanScalarToHms(null)).toBeNull();
    expect(timeSpanScalarToHms("")).toBeNull();
  });
});
