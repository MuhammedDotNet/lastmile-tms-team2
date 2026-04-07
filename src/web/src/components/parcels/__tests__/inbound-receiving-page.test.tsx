import type { ReactNode } from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { InboundReceivingPage } from "@/components/parcels/inbound-receiving-page";

const {
  mockStartInboundReceivingSession,
  mockScanInboundParcel,
  mockConfirmInboundReceivingSession,
} = vi.hoisted(() => ({
  mockStartInboundReceivingSession: vi.fn(),
  mockScanInboundParcel: vi.fn(),
  mockConfirmInboundReceivingSession: vi.fn(),
}));

let mockManifestData: unknown[] = [];
let mockQueriedSession: unknown = null;

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
  useOpenInboundManifests: () => ({
    data: mockManifestData,
    isLoading: false,
    error: null,
  }),
  useInboundReceivingSession: () => ({
    data: mockQueriedSession,
    isLoading: false,
    error: null,
  }),
  useStartInboundReceivingSession: () => ({
    mutateAsync: mockStartInboundReceivingSession,
    isPending: false,
  }),
  useScanInboundParcel: () => ({
    mutateAsync: mockScanInboundParcel,
    isPending: false,
  }),
  useConfirmInboundReceivingSession: () => ({
    mutateAsync: mockConfirmInboundReceivingSession,
    isPending: false,
  }),
}));

function makeOpenSession() {
  return {
    id: "session-1",
    manifestId: "manifest-1",
    manifestNumber: "MAN-1001",
    truckIdentifier: "TRUCK-7",
    depotId: "depot-1",
    depotName: "Sydney Inbound Depot",
    status: "Open",
    startedAt: "2026-04-07T09:00:00Z",
    startedBy: "warehouse.operator@example.com",
    confirmedAt: null,
    confirmedBy: null,
    expectedParcelCount: 2,
    scannedExpectedCount: 0,
    scannedUnexpectedCount: 1,
    remainingExpectedCount: 2,
    expectedParcels: [
      {
        manifestLineId: "line-1",
        parcelId: "parcel-1",
        trackingNumber: "LMINBOUND0001",
        barcode: "LMINBOUND0001",
        status: "Registered",
        isScanned: false,
      },
      {
        manifestLineId: "line-2",
        parcelId: "parcel-2",
        trackingNumber: "LMINBOUND0002",
        barcode: "LMINBOUND0002",
        status: "Registered",
        isScanned: false,
      },
    ],
    scannedParcels: [
      {
        id: "scan-99",
        parcelId: "parcel-99",
        trackingNumber: "LMINBOUND0099",
        barcode: "LMINBOUND0099",
        matchType: "Unexpected",
        status: "ReceivedAtDepot",
        scannedAt: "2026-04-07T09:05:00Z",
        scannedBy: "warehouse.operator@example.com",
      },
    ],
    exceptions: [
      {
        id: "exception-99",
        parcelId: "parcel-99",
        manifestLineId: null,
        exceptionType: "Unexpected",
        trackingNumber: "LMINBOUND0099",
        barcode: "LMINBOUND0099",
        createdAt: "2026-04-07T09:05:00Z",
      },
    ],
  };
}

describe("InboundReceivingPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockManifestData = [
      {
        id: "manifest-1",
        manifestNumber: "MAN-1001",
        truckIdentifier: "TRUCK-7",
        depotId: "depot-1",
        depotName: "Sydney Inbound Depot",
        status: "Open",
        expectedParcelCount: 2,
        scannedExpectedCount: 0,
        scannedUnexpectedCount: 0,
        openSessionId: null,
        createdAt: "2026-04-07T08:45:00Z",
      },
    ];
    mockQueriedSession = null;
  });

  it("starts a receiving session for the selected manifest", async () => {
    mockStartInboundReceivingSession.mockResolvedValue(makeOpenSession());

    render(<InboundReceivingPage />);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /start receiving/i }));

    await waitFor(() => {
      expect(mockStartInboundReceivingSession).toHaveBeenCalledWith({
        manifestId: "manifest-1",
      });
    });

    expect(screen.getByText("MAN-1001")).toBeInTheDocument();
    expect(screen.getByText("TRUCK-7")).toBeInTheDocument();
    expect(screen.getAllByText(/expected parcels/i).length).toBeGreaterThan(0);
    expect(screen.getByText(/remaining expected/i)).toBeInTheDocument();
    expect(screen.getAllByText("2").length).toBeGreaterThan(0);
  });

  it("submits barcode scans and renders the updated session snapshot", async () => {
    const openSession = makeOpenSession();
    mockStartInboundReceivingSession.mockResolvedValue(openSession);
    mockScanInboundParcel.mockResolvedValue({
      sessionId: "session-1",
      isExpected: true,
      scannedParcel: {
        id: "scan-1",
        parcelId: "parcel-1",
        trackingNumber: "LMINBOUND0001",
        barcode: "LMINBOUND0001",
        matchType: "Expected",
        status: "ReceivedAtDepot",
        scannedAt: "2026-04-07T09:07:00Z",
        scannedBy: "warehouse.operator@example.com",
      },
      session: {
        ...openSession,
        scannedExpectedCount: 1,
        remainingExpectedCount: 1,
        expectedParcels: openSession.expectedParcels.map((parcel: { trackingNumber: string }) =>
          parcel.trackingNumber === "LMINBOUND0001"
            ? { ...parcel, isScanned: true, status: "ReceivedAtDepot" }
            : parcel,
        ),
        scannedParcels: [
          {
            id: "scan-1",
            parcelId: "parcel-1",
            trackingNumber: "LMINBOUND0001",
            barcode: "LMINBOUND0001",
            matchType: "Expected",
            status: "ReceivedAtDepot",
            scannedAt: "2026-04-07T09:07:00Z",
            scannedBy: "warehouse.operator@example.com",
          },
          ...openSession.scannedParcels,
        ],
      },
    });

    render(<InboundReceivingPage />);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /start receiving/i }));

    await user.type(screen.getByLabelText(/scan barcode/i), "LMINBOUND0001{enter}");

    await waitFor(() => {
      expect(mockScanInboundParcel).toHaveBeenCalledWith({
        sessionId: "session-1",
        barcode: "LMINBOUND0001",
      });
    });

    expect(screen.getByText(/expected scan accepted/i)).toBeInTheDocument();
    expect(screen.getByText("LMINBOUND0001")).toBeInTheDocument();
    expect(screen.getByText(/expected parcels scanned/i)).toBeInTheDocument();
  });

  it("renders unexpected and missing sections and confirms the session", async () => {
    const openSession = makeOpenSession();
    mockStartInboundReceivingSession.mockResolvedValue(openSession);
    mockConfirmInboundReceivingSession.mockResolvedValue({
      ...openSession,
      status: "Confirmed",
      remainingExpectedCount: 0,
      exceptions: [
        ...openSession.exceptions,
        {
          id: "exception-2",
          parcelId: "parcel-2",
          manifestLineId: "line-2",
          exceptionType: "Missing",
          trackingNumber: "LMINBOUND0002",
          barcode: "LMINBOUND0002",
          createdAt: "2026-04-07T09:20:00Z",
        },
      ],
    });

    render(<InboundReceivingPage />);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /start receiving/i }));

    expect(
      screen.getByRole("heading", { name: /unexpected parcels/i }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("heading", { name: /remaining manifest parcels/i }),
    ).toBeInTheDocument();
    expect(screen.getAllByText("LMINBOUND0099").length).toBeGreaterThan(0);
    expect(screen.getAllByText("LMINBOUND0002").length).toBeGreaterThan(0);

    await user.click(screen.getByRole("button", { name: /confirm receipt/i }));

    await waitFor(() => {
      expect(mockConfirmInboundReceivingSession).toHaveBeenCalledWith({
        sessionId: "session-1",
      });
    });

    expect(screen.getByText(/session confirmed/i)).toBeInTheDocument();
    expect(screen.getAllByText("LMINBOUND0002").length).toBeGreaterThan(0);
  });
});
