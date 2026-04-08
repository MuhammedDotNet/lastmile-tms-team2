import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import BinLocationsPage from "@/components/bin-locations/bin-locations-page";

const mockUseSession = vi.fn();
const mockUseDepots = vi.fn();
const mockUseDepotStorageLayout = vi.fn();
const createStorageZoneMutateAsync = vi.fn();
const createBinLocationMutateAsync = vi.fn();
const updateBinLocationMutateAsync = vi.fn();

vi.mock("next-auth/react", () => ({
  useSession: () => mockUseSession(),
}));

vi.mock("@/queries/depots", () => ({
  useDepots: () => mockUseDepots(),
}));

vi.mock("@/queries/bin-locations", () => ({
  useDepotStorageLayout: (...args: unknown[]) =>
    mockUseDepotStorageLayout(...args),
  useCreateStorageZone: () => ({
    mutateAsync: createStorageZoneMutateAsync,
    isPending: false,
  }),
  useUpdateStorageZone: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
  useDeleteStorageZone: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
  useCreateStorageAisle: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
  useUpdateStorageAisle: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
  useDeleteStorageAisle: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
  useCreateBinLocation: () => ({
    mutateAsync: createBinLocationMutateAsync,
    isPending: false,
  }),
  useUpdateBinLocation: () => ({
    mutateAsync: updateBinLocationMutateAsync,
    isPending: false,
  }),
  useDeleteBinLocation: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
}));

describe("BinLocationsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();

    mockUseSession.mockReturnValue({
      status: "authenticated",
      data: {
        user: {
          roles: ["OperationsManager"],
        },
      },
    });

    mockUseDepots.mockReturnValue({
      data: [
        {
          id: "depot-1",
          name: "North Depot",
          isActive: true,
        },
        {
          id: "depot-2",
          name: "South Depot",
          isActive: true,
        },
      ],
      isLoading: false,
      error: null,
    });

    mockUseDepotStorageLayout.mockImplementation((depotId: string) => ({
      data:
        depotId === "depot-2"
          ? {
              depotId: "depot-2",
              depotName: "South Depot",
              availableDeliveryZones: [
                {
                  id: "delivery-zone-3",
                  name: "South Zone",
                },
              ],
              storageZones: [
                {
                  id: "zone-2",
                  name: "South Zone",
                  depotId: "depot-2",
                  storageAisles: [],
                },
              ],
            }
          : {
              depotId: "depot-1",
              depotName: "North Depot",
              availableDeliveryZones: [
                {
                  id: "delivery-zone-1",
                  name: "Metro North",
                },
                {
                  id: "delivery-zone-2",
                  name: "Metro South",
                },
              ],
              storageZones: [
                {
                  id: "zone-1",
                  name: "North Zone",
                  depotId: "depot-1",
                  storageAisles: [
                    {
                      id: "aisle-1",
                      name: "Aisle A",
                      storageZoneId: "zone-1",
                      binLocations: [
                        {
                          id: "bin-1",
                          name: "BIN-01",
                          isActive: true,
                          storageAisleId: "aisle-1",
                          deliveryZoneId: "delivery-zone-1",
                          deliveryZoneName: "Metro North",
                        },
                        {
                          id: "bin-2",
                          name: "BIN-02",
                          isActive: false,
                          storageAisleId: "aisle-1",
                          deliveryZoneId: null,
                          deliveryZoneName: null,
                        },
                      ],
                    },
                  ],
                },
              ],
            },
      isLoading: false,
      error: null,
    }));
  });

  it("renders the depot hierarchy and switches selected depots", async () => {
    render(<BinLocationsPage />);

    expect(screen.getByRole("heading", { name: /bin locations/i })).toBeInTheDocument();
    expect(screen.getByText("North Zone")).toBeInTheDocument();
    expect(screen.getByText("Aisle A")).toBeInTheDocument();
    expect(screen.getByText("BIN-01")).toBeInTheDocument();
    expect(screen.getByText("Delivery zone: Metro North")).toBeInTheDocument();

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText(/depot/i), "depot-2");

    await waitFor(() => {
      expect(screen.getByText("South Zone")).toBeInTheDocument();
    });
  });

  it("creates a storage zone for the selected depot", async () => {
    createStorageZoneMutateAsync.mockResolvedValue({
      id: "zone-3",
      name: "Overflow Zone",
      depotId: "depot-2",
      storageAisles: [],
    });

    render(<BinLocationsPage />);

    const user = userEvent.setup();
    await user.selectOptions(screen.getByLabelText(/depot/i), "depot-2");
    await user.click(screen.getByRole("button", { name: /add storage zone/i }));
    await user.type(screen.getByLabelText(/storage zone name/i), "Overflow Zone");
    await user.click(screen.getByRole("button", { name: /create storage zone/i }));

    await waitFor(() => {
      expect(createStorageZoneMutateAsync).toHaveBeenCalledWith({
        depotId: "depot-2",
        name: "Overflow Zone",
      });
    });
  });

  it("disables taken delivery zones for active bins and allows reuse for inactive bins", async () => {
    createBinLocationMutateAsync.mockResolvedValue({
      id: "bin-3",
      name: "BIN-03",
      isActive: false,
      storageAisleId: "aisle-1",
      deliveryZoneId: "delivery-zone-1",
      deliveryZoneName: "Metro North",
    });

    render(<BinLocationsPage />);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /add bin/i }));

    let deliveryZoneSelect = screen.getByLabelText(/delivery zone/i);
    expect(
      within(deliveryZoneSelect).getByRole("option", { name: "Metro North" }),
    ).toBeDisabled();
    expect(
      within(deliveryZoneSelect).getByRole("option", { name: "Metro South" }),
    ).not.toBeDisabled();

    await user.selectOptions(screen.getByLabelText(/status/i), "false");

    await waitFor(() => {
      deliveryZoneSelect = screen.getByLabelText(/delivery zone/i);
      expect(
        within(deliveryZoneSelect).getByRole("option", { name: "Metro North" }),
      ).not.toBeDisabled();
    });

    await user.type(screen.getByLabelText(/bin name/i), "BIN-03");
    await user.selectOptions(deliveryZoneSelect, "delivery-zone-1");
    await user.click(screen.getByRole("button", { name: /create bin location/i }));

    await waitFor(() => {
      expect(createBinLocationMutateAsync).toHaveBeenCalledWith({
        storageAisleId: "aisle-1",
        name: "BIN-03",
        isActive: false,
        deliveryZoneId: "delivery-zone-1",
      });
    });
  });

  it("keeps the current bin delivery zone selectable while editing", async () => {
    render(<BinLocationsPage />);

    const user = userEvent.setup();
    await user.click(screen.getByRole("button", { name: /edit bin bin-01/i }));

    const deliveryZoneSelect = screen.getByLabelText(/delivery zone/i);
    expect(deliveryZoneSelect).toHaveValue("delivery-zone-1");
    expect(
      within(deliveryZoneSelect).getByRole("option", { name: "Metro North" }),
    ).not.toBeDisabled();
  });

  it("renders an accessible delete dialog and closes it on Escape", async () => {
    render(<BinLocationsPage />);

    const user = userEvent.setup();
    await user.click(screen.getAllByRole("button", { name: /delete/i })[0]);

    expect(screen.getByRole("dialog")).toBeInTheDocument();

    await user.keyboard("{Escape}");

    await waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });
  });
});
