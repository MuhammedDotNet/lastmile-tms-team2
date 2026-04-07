import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import BinLocationsPage from "@/components/bin-locations/bin-locations-page";

const mockUseSession = vi.fn();
const mockUseDepots = vi.fn();
const mockUseDepotStorageLayout = vi.fn();
const createStorageZoneMutateAsync = vi.fn();

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
    mutateAsync: vi.fn(),
    isPending: false,
  }),
  useUpdateBinLocation: () => ({
    mutateAsync: vi.fn(),
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
});
