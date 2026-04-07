import type { ReactNode } from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { RouteStagingPage } from "@/components/parcels/route-staging-page";

const { mockStageParcelForRoute } = vi.hoisted(() => ({
  mockStageParcelForRoute: vi.fn(),
}));

let mockStagingRoutes: unknown[] = [];
let mockRouteStagingBoard: unknown = null;

vi.mock("next-auth/react", () => ({
  useSession: () => ({
    status: "authenticated",
    data: { user: { name: "Warehouse Operator" } },
  }),
}));

vi.mock("@/components/ui/label", () => ({
  Label: ({
    children,
    htmlFor,
  }: {
    children: ReactNode;
    htmlFor?: string;
  }) => <label htmlFor={htmlFor}>{children}</label>,
}));

vi.mock("@/queries/parcels", () => ({
  useStagingRoutes: () => ({
    data: mockStagingRoutes,
    isLoading: false,
    error: null,
  }),
  useRouteStagingBoard: () => ({
    data: mockRouteStagingBoard,
    isLoading: false,
    error: null,
  }),
  useStageParcelForRoute: () => ({
    mutateAsync: mockStageParcelForRoute,
    isPending: false,
  }),
}));

function makeBoard() {
  return {
    id: "route-1",
    vehicleId: "vehicle-1",
    vehiclePlate: "TRUCK-101",
    driverId: "driver-1",
    driverName: "Jamie Parker",
    status: "PLANNED",
    stagingArea: "A",
    startDate: "2026-04-07T08:30:00Z",
    expectedParcelCount: 2,
    stagedParcelCount: 1,
    remainingParcelCount: 1,
    expectedParcels: [
      {
        parcelId: "parcel-1",
        trackingNumber: "LMSTAGEWEB1001",
        barcode: "LMSTAGEWEB1001",
        status: "Staged",
        isStaged: true,
      },
      {
        parcelId: "parcel-2",
        trackingNumber: "LMSTAGEWEB1002",
        barcode: "LMSTAGEWEB1002",
        status: "Sorted",
        isStaged: false,
      },
    ],
  };
}

describe("RouteStagingPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockStagingRoutes = [
      {
        id: "route-1",
        vehicleId: "vehicle-1",
        vehiclePlate: "TRUCK-101",
        driverId: "driver-1",
        driverName: "Jamie Parker",
        status: "PLANNED",
        stagingArea: "A",
        startDate: "2026-04-07T08:30:00Z",
        expectedParcelCount: 2,
        stagedParcelCount: 1,
        remainingParcelCount: 1,
      },
    ];
    mockRouteStagingBoard = makeBoard();
  });

  it("submits typed scans for the selected route and renders the updated board", async () => {
    mockStageParcelForRoute.mockResolvedValue({
      outcome: "STAGED",
      message: "Parcel staged successfully.",
      trackingNumber: "LMSTAGEWEB1002",
      parcelId: "parcel-2",
      conflictingRouteId: null,
      conflictingStagingArea: null,
      board: {
        ...makeBoard(),
        stagedParcelCount: 2,
        remainingParcelCount: 0,
        expectedParcels: makeBoard().expectedParcels.map((parcel: { trackingNumber: string }) =>
          parcel.trackingNumber === "LMSTAGEWEB1002"
            ? { ...parcel, status: "Staged", isStaged: true }
            : parcel,
        ),
      },
    });

    render(<RouteStagingPage />);

    const user = userEvent.setup();

    expect(screen.getByText(/route staging/i)).toBeInTheDocument();
    expect(screen.getByText(/area a/i)).toBeInTheDocument();
    expect(screen.getByText(/expected parcels/i)).toBeInTheDocument();

    await user.type(screen.getByLabelText(/scan barcode/i), "LMSTAGEWEB1002{enter}");

    await waitFor(() => {
      expect(mockStageParcelForRoute).toHaveBeenCalledWith({
        routeId: "route-1",
        barcode: "LMSTAGEWEB1002",
      });
    });

    expect(screen.getByText(/parcel staged successfully/i)).toBeInTheDocument();
    expect(screen.getByText(/remaining parcels/i)).toBeInTheDocument();
  });

  it("renders a wrong-route warning with the conflicting staging area", async () => {
    mockStageParcelForRoute.mockResolvedValue({
      outcome: "WRONG_ROUTE",
      message: "Parcel is assigned to a different active route.",
      trackingNumber: "LMSTAGEWEB9999",
      parcelId: "parcel-9",
      conflictingRouteId: "route-9",
      conflictingStagingArea: "B",
      board: makeBoard(),
    });

    render(<RouteStagingPage />);

    const user = userEvent.setup();
    await user.type(screen.getByLabelText(/scan barcode/i), "LMSTAGEWEB9999{enter}");

    await waitFor(() => {
      expect(mockStageParcelForRoute).toHaveBeenCalledWith({
        routeId: "route-1",
        barcode: "LMSTAGEWEB9999",
      });
    });

    expect(
      screen.getByText(/parcel is assigned to a different active route/i),
    ).toBeInTheDocument();
    expect(screen.getByText(/area b/i)).toBeInTheDocument();
  });
});
