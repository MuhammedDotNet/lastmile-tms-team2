import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import NewRoutePage from "@/components/routes/route-create-page";

const { mockPush, mockCreateRoute } = vi.hoisted(() => ({
  mockPush: vi.fn(),
  mockCreateRoute: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

vi.mock("@/queries/routes", () => ({
  useCreateRoute: () => ({
    mutateAsync: mockCreateRoute,
    isPending: false,
  }),
}));

vi.mock("@/queries/vehicles", () => ({
  useVehicles: () => ({
    data: [
      {
        id: "vehicle-1",
        registrationPlate: "TRUCK-101",
        type: "VAN",
        parcelCapacity: 12,
        weightCapacity: 180,
        status: "AVAILABLE",
        depotId: "depot-1",
        depotName: "Test Depot",
        totalRoutes: 0,
        routesCompleted: 0,
        totalMileage: 0,
        createdAt: "2026-04-07T08:00:00Z",
        updatedAt: null,
      },
    ],
  }),
}));

vi.mock("@/queries/drivers", () => ({
  useDrivers: () => ({
    data: [
      {
        id: "driver-1",
        displayName: "Jamie Parker",
      },
    ],
    isLoading: false,
    error: null,
  }),
}));

vi.mock("@/queries/parcels", () => ({
  useParcelsForRouteCreation: () => ({
    data: [
      {
        id: "parcel-1",
        trackingNumber: "LMSTAGEWEB0001",
        weight: 2.5,
        weightUnit: 1,
        zoneId: "zone-1",
        zoneName: "Zone A",
      },
    ],
    isLoading: false,
    error: null,
  }),
}));

describe("route-create-page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockCreateRoute.mockResolvedValue({ id: "route-1" });
  });

  it("submits the selected staging area with the create route request", async () => {
    render(<NewRoutePage />);

    const user = userEvent.setup();

    await user.click(screen.getByRole("button", { name: /select vehicle/i }));
    await user.click(
      screen.getByRole("option", { name: /truck-101/i }),
    );

    await user.click(screen.getByRole("button", { name: /select driver/i }));
    await user.click(
      screen.getByRole("option", { name: /jamie parker/i }),
    );

    await user.click(
      screen.getByRole("button", { name: /select staging area/i }),
    );
    await user.click(screen.getByRole("option", { name: /area b/i }));

    await user.click(screen.getByLabelText(/lmstageweb0001/i));
    await user.click(screen.getByRole("button", { name: /create route/i }));

    await waitFor(() => {
      expect(mockCreateRoute).toHaveBeenCalledWith(
        expect.objectContaining({
          vehicleId: "vehicle-1",
          driverId: "driver-1",
          stagingArea: "B",
          parcelIds: ["parcel-1"],
          startMileage: 0,
          startDate: expect.any(String),
        }),
      );
    });

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/routes");
    });
  });
});
