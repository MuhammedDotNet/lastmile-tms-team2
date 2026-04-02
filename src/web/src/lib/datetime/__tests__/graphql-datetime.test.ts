import { describe, expect, it } from "vitest";

import {
  toDateInputValue,
  toGraphQLDateTimeFromDateInput,
} from "../graphql-datetime";

describe("toGraphQLDateTimeFromDateInput", () => {
  it("extends YYYY-MM-DD to noon UTC ISO", () => {
    expect(toGraphQLDateTimeFromDateInput("2026-04-15")).toBe(
      "2026-04-15T12:00:00.000Z",
    );
  });

  it("passes through full ISO strings", () => {
    expect(toGraphQLDateTimeFromDateInput("2026-04-15T00:00:00.000Z")).toBe(
      "2026-04-15T00:00:00.000Z",
    );
  });
});

describe("toDateInputValue", () => {
  it("extracts date from ISO", () => {
    expect(toDateInputValue("2026-04-15T12:00:00.000Z")).toBe("2026-04-15");
  });
});
