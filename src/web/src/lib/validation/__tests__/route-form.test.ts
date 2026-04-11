import { describe, expect, it } from "vitest";

import { routeCreateFormSchema } from "../routes";

const id = "550e8400-e29b-41d4-a716-446655440000";

describe("routeCreateFormSchema", () => {
  it("accepts valid route form", () => {
    const r = routeCreateFormSchema.safeParse({
      zoneId: id,
      vehicleId: id,
      driverId: id,
      stagingArea: "A",
      startDate: "2025-01-15T10:30",
      startMileage: 0,
      assignmentMode: "MANUAL_PARCELS",
      stopMode: "AUTO",
      parcelIds: [],
      stops: [],
    });
    expect(r.success).toBe(true);
  });

  it("rejects negative mileage", () => {
    const r = routeCreateFormSchema.safeParse({
      zoneId: id,
      vehicleId: id,
      driverId: id,
      stagingArea: "A",
      startDate: "2025-01-15T10:30",
      startMileage: -1,
      assignmentMode: "MANUAL_PARCELS",
      stopMode: "AUTO",
      parcelIds: [],
      stops: [],
    });
    expect(r.success).toBe(false);
  });
});
