import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import ParcelDetailPage from "@/components/parcels/parcel-detail-page";

const {
  mockCancelParcel,
  mockDownloadLabel,
  mockReplace,
  mockTransitionParcelStatus,
} = vi.hoisted(() => ({
  mockCancelParcel: vi.fn(),
  mockDownloadLabel: vi.fn(),
  mockReplace: vi.fn(),
  mockTransitionParcelStatus: vi.fn(),
}));

let mockParcelKey = "LM202604010001";

vi.mock("next-auth/react", () => ({
  useSession: () => ({
    status: "authenticated",
    data: { user: { name: "Warehouse User" } },
  }),
}));

vi.mock("next/navigation", () => ({
  useParams: () => ({ id: mockParcelKey }),
  useRouter: () => ({
    replace: mockReplace,
    push: vi.fn(),
    back: vi.fn(),
  }),
}));

vi.mock("@/queries/parcels", () => ({
  useParcelRealtimeUpdates: vi.fn(),
  useDownloadParcelLabel: () => ({
    mutateAsync: mockDownloadLabel,
    isPending: false,
  }),
  useParcel: () => ({
    data: {
      id: "parcel-1",
      trackingNumber: "LM202604010001",
      barcode: "LM202604010001",
      status: "REGISTERED",
      shipperAddressId: "address-1",
      serviceType: "STANDARD",
      weight: 2.5,
      weightUnit: "KG",
      length: 30,
      width: 20,
      height: 15,
      dimensionUnit: "CM",
      declaredValue: 120,
      currency: "USD",
      description: "Warehouse intake parcel",
      parcelType: "Box",
      cancellationReason: null,
      estimatedDeliveryDate: "2026-04-08T00:00:00Z",
      deliveryAttempts: 0,
      zoneId: "zone-1",
      zoneName: "North Zone",
      depotId: "depot-1",
      depotName: "North Depot",
      createdAt: "2026-04-01T09:15:00Z",
      lastModifiedAt: "2026-04-01T10:00:00Z",
      canEdit: true,
      canCancel: true,
      senderAddress: {
        street1: "10 Shipper Lane",
        street2: "Dock Office",
        city: "Chicago",
        state: "IL",
        postalCode: "60601",
        countryCode: "US",
        isResidential: false,
        contactName: "Sender Contact",
        companyName: "Warehouse Sender",
        phone: "+1 555 0199",
        email: "sender@example.com",
      },
      recipientAddress: {
        street1: "123 Main St",
        street2: null,
        city: "Springfield",
        state: "IL",
        postalCode: "62701",
        countryCode: "US",
        isResidential: true,
        contactName: "Jamie Carter",
        companyName: null,
        phone: "+1 555 0100",
        email: "jamie@example.com",
      },
      statusTimeline: [
        {
          id: "evt-2",
          timestamp: "2026-04-01T11:30:00Z",
          eventType: "OutForDelivery",
          description: "Out on route",
          location: "Dock 3",
          operator: "Dispatch User",
        },
        {
          id: "evt-1",
          timestamp: "2026-04-01T09:15:00Z",
          eventType: "ArrivedAtFacility",
          description: "Parcel received",
          location: "North Depot",
          operator: "Warehouse User",
        },
      ],
      changeHistory: [
        {
          action: "Updated",
          fieldName: "Description",
          beforeValue: "Old note",
          afterValue: "Warehouse intake parcel",
          changedAt: "2026-04-01T10:00:00Z",
          changedBy: "Warehouse User",
        },
      ],
      routeAssignment: {
        routeId: "route-1",
        routeStatus: "InProgress",
        startDate: "2026-04-01T10:30:00Z",
        endDate: null,
        driverId: "driver-1",
        driverName: "Jamie Driver",
        vehicleId: "vehicle-1",
        vehiclePlate: "TST-100",
      },
      proofOfDelivery: {
        receivedBy: "Front Desk",
        deliveryLocation: "Building A",
        deliveredAt: "2026-04-01T12:45:00Z",
        hasSignatureImage: true,
        hasPhoto: true,
      },
      allowedNextStatuses: ["RECEIVED_AT_DEPOT", "CANCELLED"],
    },
    isLoading: false,
    error: null,
  }),
  useCancelParcel: () => ({
    mutateAsync: mockCancelParcel,
    isPending: false,
  }),
  useTransitionParcelStatus: () => ({
    mutateAsync: mockTransitionParcelStatus,
    isPending: false,
  }),
}));

describe("ParcelDetailPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockParcelKey = "LM202604010001";
  });

  it("renders sender, recipient, timeline, route, proof of delivery, and reprint actions", async () => {
    render(<ParcelDetailPage />);

    expect(
      screen.getByRole("heading", { name: "LM202604010001" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Sender" })).toBeInTheDocument();
    expect(screen.getByText("Sender Contact")).toBeInTheDocument();
    expect(screen.getByText("Jamie Carter")).toBeInTheDocument();
    expect(screen.getAllByText("North Zone").length).toBeGreaterThan(0);
    expect(screen.getByText("+1 555 0100")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: /edit parcel/i })).toBeInTheDocument();
    expect(screen.getByText(/change history/i)).toBeInTheDocument();
    expect(screen.getByText(/status timeline/i)).toBeInTheDocument();
    expect(screen.getByText("Out on route")).toBeInTheDocument();
    expect(screen.getByText(/related route/i)).toBeInTheDocument();
    expect(screen.getByText("Jamie Driver")).toBeInTheDocument();
    expect(screen.getByText("TST-100")).toBeInTheDocument();
    expect(screen.getByText(/proof of delivery/i)).toBeInTheDocument();
    expect(screen.getByText("Front Desk")).toBeInTheDocument();
    expect(screen.getByText("Description")).toBeInTheDocument();
    expect(screen.getByText("Old note")).toBeInTheDocument();
    expect(screen.getAllByText("Warehouse intake parcel").length).toBeGreaterThan(0);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /reprint 4x6 zpl/i }));
    await user.click(screen.getByRole("button", { name: /reprint a4 pdf/i }));

    await waitFor(() => {
      expect(mockDownloadLabel).toHaveBeenNthCalledWith(1, {
        parcelId: "parcel-1",
        format: "zpl",
      });
      expect(mockDownloadLabel).toHaveBeenNthCalledWith(2, {
        parcelId: "parcel-1",
        format: "pdf",
      });
    });
  });

  it("redirects GUID parcel keys to the canonical tracking-number URL", async () => {
    mockParcelKey = "40000000-0000-0000-0000-000000000001";

    render(<ParcelDetailPage />);

    await waitFor(() => {
      expect(mockReplace).toHaveBeenCalledWith("/parcels/LM202604010001");
    });
  });

  it("requires a cancellation reason before cancelling from detail view", async () => {
    render(<ParcelDetailPage />);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /^cancel parcel$/i }));
    await user.click(screen.getAllByRole("button", { name: /^cancel parcel$/i })[1]);

    expect(screen.getByText(/cancellation reason is required/i)).toBeInTheDocument();

    await user.type(screen.getByLabelText(/cancellation reason/i), "Customer request");
    await user.click(screen.getAllByRole("button", { name: /^cancel parcel$/i })[1]);

    await waitFor(() => {
      expect(mockCancelParcel).toHaveBeenCalledWith({
        id: "parcel-1",
        reason: "Customer request",
      });
    });
  });
});
