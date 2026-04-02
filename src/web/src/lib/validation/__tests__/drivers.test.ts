import { describe, expect, it } from "vitest";

import { driverCreateFormSchema } from "../drivers";

const id = "550e8400-e29b-41d4-a716-446655440000";

const base = {
  firstName: "Ali",
  lastName: "Ahmed",
  phone: null,
  email: null,
  licenseNumber: "LIC-1",
  licenseExpiryDate: null,
  zoneId: id,
  depotId: id,
  status: "ACTIVE" as const,
  userId: id,
  availabilitySchedule: [
    {
      dayOfWeek: "MONDAY" as const,
      shiftStart: "08:00:00",
      shiftEnd: "17:00:00",
      isAvailable: true,
    },
    {
      dayOfWeek: "TUESDAY" as const,
      shiftStart: "08:00:00",
      shiftEnd: "17:00:00",
      isAvailable: true,
    },
  ],
};

describe("driverCreateFormSchema", () => {
  it("accepts valid driver form", () => {
    const r = driverCreateFormSchema.safeParse(base);
    expect(r.success).toBe(true);
  });

  it("rejects duplicate day of week in availability", () => {
    const r = driverCreateFormSchema.safeParse({
      ...base,
      availabilitySchedule: [
        {
          dayOfWeek: "MONDAY" as const,
          shiftStart: "08:00:00",
          shiftEnd: "17:00:00",
          isAvailable: true,
        },
        {
          dayOfWeek: "MONDAY" as const,
          shiftStart: "09:00:00",
          shiftEnd: "18:00:00",
          isAvailable: false,
        },
      ],
    });
    expect(r.success).toBe(false);
    if (!r.success) {
      expect(r.error.issues.some((i) => i.path[0] === "availabilitySchedule")).toBe(
        true,
      );
    }
  });

  it("allows expired license date string (backend records reality)", () => {
    const r = driverCreateFormSchema.safeParse({
      ...base,
      licenseExpiryDate: "2020-01-01",
    });
    expect(r.success).toBe(true);
  });
});
