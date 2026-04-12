import { describe, expect, it } from "vitest";

import {
  buildDispatchMapDayRange,
  computeDispatchMapStopStatus,
} from "@/lib/routes/dispatch-map";

describe("dispatch map helpers", () => {
  it("builds a half-open local day range from YYYY-MM-DD", () => {
    const range = buildDispatchMapDayRange("2026-04-12");

    expect(range).toEqual({
      gte: new Date(2026, 3, 12).toISOString(),
      lt: new Date(2026, 3, 13).toISOString(),
    });
  });

  it("marks a stop as delivered when every parcel is delivered", () => {
    const status = computeDispatchMapStopStatus([
      { status: "DELIVERED" },
      { status: "DELIVERED" },
    ]);

    expect(status).toBe("DELIVERED");
  });

  it("marks a stop as failed when any parcel is failed or returned", () => {
    expect(
      computeDispatchMapStopStatus([
        { status: "OUT_FOR_DELIVERY" },
        { status: "FAILED_ATTEMPT" },
      ]),
    ).toBe("FAILED");

    expect(
      computeDispatchMapStopStatus([
        { status: "DELIVERED" },
        { status: "RETURNED_TO_DEPOT" },
      ]),
    ).toBe("FAILED");
  });

  it("marks a stop as waiting for all other parcel combinations", () => {
    const status = computeDispatchMapStopStatus([
      { status: "SORTED" },
      { status: "OUT_FOR_DELIVERY" },
    ]);

    expect(status).toBe("WAITING");
  });
});
