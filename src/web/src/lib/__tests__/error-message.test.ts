import { describe, it, expect } from "vitest";

import { getErrorMessage } from "../error-message";

describe("getErrorMessage", () => {
  it("returns Error.message", () => {
    expect(getErrorMessage(new Error("Bad request"))).toBe("Bad request");
  });

  it("appends cause message when present", () => {
    const err = new Error("Request failed");
    (err as Error & { cause: Error }).cause = new Error("Invalid depot");
    expect(getErrorMessage(err)).toBe("Request failed: Invalid depot");
  });

  it("reads string errors", () => {
    expect(getErrorMessage("Token expired")).toBe("Token expired");
  });

  it("reads message on plain object", () => {
    expect(getErrorMessage({ message: "GraphQL validation failed" })).toBe(
      "GraphQL validation failed",
    );
  });
});
