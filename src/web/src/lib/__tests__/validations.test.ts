import { describe, it, expect } from "vitest";
import { loginSchema } from "../validations";

describe("loginSchema", () => {
  it("should validate correct login data", () => {
    const validData = {
      email: "user@example.com",
      password: "password123",
    };

    const result = loginSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it("should reject empty email", () => {
    const invalidData = {
      email: "",
      password: "password123",
    };

    const result = loginSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toBe("Email is required");
    }
  });

  it("should reject invalid email format", () => {
    const invalidData = {
      email: "not-an-email",
      password: "password123",
    };

    const result = loginSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
  });
});
