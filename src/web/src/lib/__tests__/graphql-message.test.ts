import { describe, it, expect } from "vitest";

import { messageFromGraphQLError } from "../graphql";

describe("messageFromGraphQLError", () => {
  it("returns normal messages as-is", () => {
    expect(
      messageFromGraphQLError({ message: "Vehicle not found" }),
    ).toBe("Vehicle not found");
  });

  it("reads extensions when top-level message is Hot Chocolate generic", () => {
    expect(
      messageFromGraphQLError({
        message: "Unexpected Execution Error",
        extensions: {
          message:
            "Cannot set vehicle to Available while it has a planned or in-progress route.",
        },
      }),
    ).toBe(
      "Cannot set vehicle to Available while it has a planned or in-progress route.",
    );
  });
});
