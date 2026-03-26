import { describe, expect, it } from "vitest";

import {
  vehicleCreateFormSchema,
  vehicleEditFormSchema,
} from "../vehicles";

const validUuid = "550e8400-e29b-41d4-a716-446655440000";

describe("vehicleCreateFormSchema", () => {
  it("accepts valid create payload", () => {
    const r = vehicleCreateFormSchema.safeParse({
      registrationPlate: "AB 123 CD",
      type: "VAN",
      parcelCapacity: 10,
      weightCapacity: 500.5,
      status: "AVAILABLE",
      depotId: validUuid,
    });
    expect(r.success).toBe(true);
  });

  it("rejects non-Available status on create", () => {
    const r = vehicleCreateFormSchema.safeParse({
      registrationPlate: "AB 123 CD",
      type: "VAN",
      parcelCapacity: 10,
      weightCapacity: 100,
      status: "IN_USE",
      depotId: validUuid,
    });
    expect(r.success).toBe(false);
  });

  it("rejects parcel capacity below 1", () => {
    const r = vehicleCreateFormSchema.safeParse({
      registrationPlate: "AB 123 CD",
      type: "VAN",
      parcelCapacity: 0,
      weightCapacity: 100,
      status: "AVAILABLE",
      depotId: validUuid,
    });
    expect(r.success).toBe(false);
  });

  it("accepts seeded Test Depot id (Zod uuid() rejects this .NET-style GUID)", () => {
    const r = vehicleCreateFormSchema.safeParse({
      registrationPlate: "AB 123 CD",
      type: "VAN",
      parcelCapacity: 10,
      weightCapacity: 100,
      status: "AVAILABLE",
      depotId: "00000000-0000-0000-0000-000000000001",
    });
    expect(r.success).toBe(true);
  });
});

describe("vehicleEditFormSchema", () => {
  it("allows InUse status", () => {
    const r = vehicleEditFormSchema.safeParse({
      registrationPlate: "AB 123 CD",
      type: "VAN",
      parcelCapacity: 10,
      weightCapacity: 100,
      status: "IN_USE",
      depotId: validUuid,
    });
    expect(r.success).toBe(true);
  });
});
