import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import NewRoutePage from "@/components/routes/route-create-page";

const { mockPush, mockCreateRoute, mockCandidates, mockPreview } = vi.hoisted(() => ({
  mockPush: vi.fn(),
  mockCreateRoute: vi.fn(),
  mockCandidates: vi.fn(),
  mockPreview: vi.fn(),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: mockPush,
  }),
}));

vi.mock("next-auth/react", () => ({
  useSession: () => ({
    status: "authenticated",
    data: { user: { name: "Dispatch User" } },
  }),
}));

vi.mock("@/components/routes/route-map", () => ({
  RouteMap: () => <div data-testid="route-map" />,
}));

vi.mock("@/queries/routes", () => ({
  useCreateRoute: () => ({
    mutateAsync: mockCreateRoute,
    isPending: false,
  }),
  useRouteAssignmentCandidates: () => mockCandidates(),
  useRoutePlanPreview: (request: unknown) => mockPreview(request),
}));

vi.mock("@/queries/zones", () => ({
  useZones: () => ({
    data: [
      {
        id: "zone-1",
        name: "Zone A",
        zoneId: "zone-1",
        depotId: "depot-1",
        depotName: "Test Depot",
        isActive: true,
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
    mockCandidates.mockReturnValue({
      data: {
        vehicles: [
          {
            id: "vehicle-1",
            registrationPlate: "TRUCK-101",
            parcelCapacity: 12,
            weightCapacity: 180,
            status: "AVAILABLE",
            depotId: "depot-1",
            depotName: "Test Depot",
            isCurrentAssignment: false,
          },
        ],
        drivers: [
          {
            id: "driver-1",
            displayName: "Jamie Parker",
            depotId: "depot-1",
            zoneId: "zone-1",
            status: "ACTIVE",
            isCurrentAssignment: false,
            workloadRoutes: [
              {
                routeId: "route-2-abcdef",
                vehicleId: "vehicle-2",
                vehiclePlate: "TRUCK-202",
                startDate: "2026-04-09T09:00:00Z",
                status: "COMPLETED",
              },
            ],
          },
        ],
      },
      isLoading: false,
      error: null,
    });
    mockPreview.mockImplementation(
      (request?: {
        assignmentMode?: "MANUAL_PARCELS" | "AUTO_BY_ZONE";
        parcelIds?: string[];
        zoneId?: string;
      }) => {
        if (!request?.zoneId) {
          return {
            data: null,
            isLoading: false,
            error: null,
          };
        }

        const isSelected =
          request.assignmentMode === "AUTO_BY_ZONE"
          || (request.parcelIds ?? []).includes("parcel-1");

        return {
          data: {
            zoneId: "zone-1",
            zoneName: "Zone A",
            depotId: "depot-1",
            depotName: "Test Depot",
            depotAddressLine: "1 Depot Street",
            depotLongitude: 151.2093,
            depotLatitude: -33.8688,
            estimatedStopCount: isSelected ? 1 : 0,
            plannedDistanceMeters: 12500,
            plannedDurationSeconds: 2100,
            warnings: [],
            candidateParcels: [
              {
                id: "parcel-1",
                trackingNumber: "LMSTAGEWEB0001",
                weight: 2.5,
                weightUnit: "KG",
                zoneId: "zone-1",
                zoneName: "Zone A",
                recipientLabel: "Recipient One",
                addressLine: "20 Recipient Road",
                longitude: 151.215,
                latitude: -33.872,
                isSelected,
              },
            ],
            stops: isSelected
              ? [
                  {
                    id: "stop-1",
                    sequence: 1,
                    recipientLabel: "Recipient One",
                    addressLine: "20 Recipient Road",
                    longitude: 151.215,
                    latitude: -33.872,
                    parcels: [
                      {
                        parcelId: "parcel-1",
                        trackingNumber: "LMSTAGEWEB0001",
                        recipientLabel: "Recipient One",
                        addressLine: "20 Recipient Road",
                      },
                    ],
                  },
                ]
              : [],
            path: [
              { longitude: 151.2093, latitude: -33.8688 },
              { longitude: 151.215, latitude: -33.872 },
            ],
          },
          isLoading: false,
          error: null,
        };
      },
    );
  });

  it("submits the selected staging area with the create route request", async () => {
    render(<NewRoutePage />);

    const user = userEvent.setup();

    await user.click(screen.getByLabelText(/^zone$/i));
    await user.click(screen.getByRole("option", { name: /zone a/i }));

    await user.click(screen.getByLabelText(/^vehicle$/i));
    await user.click(screen.getByRole("option", { name: /truck-101/i }));

    await user.click(screen.getByLabelText(/^driver$/i));
    await user.click(screen.getByRole("option", { name: /jamie parker/i }));

    await user.click(screen.getByLabelText(/^staging area$/i));
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

  it("shows workload details for the selected driver", async () => {
    render(<NewRoutePage />);

    const user = userEvent.setup();

    await user.click(screen.getByLabelText(/^zone$/i));
    await user.click(screen.getByRole("option", { name: /zone a/i }));

    await user.click(screen.getByLabelText(/^vehicle$/i));
    await user.click(screen.getByRole("option", { name: /truck-101/i }));

    await user.click(screen.getByLabelText(/^driver$/i));
    await user.click(screen.getByRole("option", { name: /jamie parker/i }));

    expect(screen.getByText(/driver workload/i)).toBeInTheDocument();
    expect(screen.getByText(/truck-202/i)).toBeInTheDocument();
    expect(screen.getByText(/completed/i)).toBeInTheDocument();
  });
});
