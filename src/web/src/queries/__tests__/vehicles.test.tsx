import { describe, it, expect, vi, beforeEach } from "vitest";
import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  useVehicles,
  useVehicle,
  useCreateVehicle,
  useUpdateVehicle,
  useDeleteVehicle,
  vehicleKeys,
} from "../vehicles";
import type { PaginatedResponse, Vehicle } from "../../types/vehicles";
import { VehicleStatus } from "../../types/vehicles";
import * as vehiclesService from "../../services/vehicles.service";

vi.mock("next-auth/react", () => ({
  useSession: () => ({
    status: "authenticated",
    data: { user: { name: "Test" } },
  }),
}));

// Mock the service
vi.mock("../../services/vehicles.service", () => ({
  vehiclesService: {
    getAll: vi.fn(),
    getById: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    delete: vi.fn(),
  },
}));

const mockVehiclesService = vi.mocked(vehiclesService.vehiclesService);

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
      },
    },
  });

  function QueryClientWrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  }
  QueryClientWrapper.displayName = "QueryClientWrapper";
  return QueryClientWrapper;
};

describe("useVehicles", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should fetch vehicles with default params", async () => {
    const mockResponse = {
      items: [{ id: "1", registrationPlate: "ABC-001" }],
      totalCount: 1,
      page: 1,
      pageSize: 20,
      totalPages: 1,
    };

    mockVehiclesService.getAll.mockResolvedValueOnce(
      mockResponse as PaginatedResponse<Vehicle>,
    );

    const { result } = renderHook(() => useVehicles({}), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockVehiclesService.getAll).toHaveBeenCalledWith(
      undefined,
      undefined,
      undefined,
      undefined
    );
    expect(result.current.data).toEqual(mockResponse);
  });

  it("should fetch vehicles with filters", async () => {
    const mockResponse = {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: 10,
      totalPages: 0,
    };

    mockVehiclesService.getAll.mockResolvedValueOnce(
      mockResponse as PaginatedResponse<Vehicle>,
    );

    const { result } = renderHook(
      () => useVehicles({ page: 2, pageSize: 10, status: VehicleStatus.Available }),
      { wrapper: createWrapper() }
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockVehiclesService.getAll).toHaveBeenCalledWith(
      2,
      10,
      VehicleStatus.Available,
      undefined
    );
  });

  it("should have correct query key", () => {
    const keys = vehicleKeys.list({ page: 1, status: VehicleStatus.Available });
    expect(keys).toEqual(["vehicles", "list", { page: 1, status: VehicleStatus.Available }]);
  });
});

describe("useVehicle", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should fetch vehicle by ID", async () => {
    const mockVehicle = {
      id: "123",
      registrationPlate: "ABC-001",
    };

    mockVehiclesService.getById.mockResolvedValueOnce(
      mockVehicle as unknown as Vehicle,
    );

    const { result } = renderHook(() => useVehicle("123"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockVehiclesService.getById).toHaveBeenCalledWith("123");
    expect(result.current.data).toEqual(mockVehicle);
  });

  it("should not fetch when ID is empty", () => {
    renderHook(() => useVehicle(""), {
      wrapper: createWrapper(),
    });

    expect(mockVehiclesService.getById).not.toHaveBeenCalled();
  });
});

describe("useCreateVehicle", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should create a vehicle and invalidate queries", async () => {
    const newVehicle = {
      registrationPlate: "XYZ-999",
      type: 0,
      parcelCapacity: 10,
      weightCapacity: 100,
      status: 0,
      depotId: "dep-1",
    };

    const createdVehicle = { id: "new-id", ...newVehicle };

    mockVehiclesService.create.mockResolvedValueOnce(
      createdVehicle as unknown as Vehicle,
    );

    const queryClient = new QueryClient();
    queryClient.setQueryData(vehicleKeys.lists(), { items: [], totalCount: 0 });

    const { result } = renderHook(() => useCreateVehicle(), {
      wrapper: ({ children }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      ),
    });

    result.current.mutate(newVehicle);

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockVehiclesService.create).toHaveBeenCalledWith(newVehicle);
  });
});

describe("useUpdateVehicle", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should update a vehicle and invalidate queries", async () => {
    const updateData = {
      registrationPlate: "ABC-001",
      type: 0,
      parcelCapacity: 15,
      weightCapacity: 150,
      status: 1,
      depotId: "dep-1",
    };

    const updatedVehicle = { id: "123", ...updateData };

    mockVehiclesService.update.mockResolvedValueOnce(
      updatedVehicle as unknown as Vehicle,
    );

    const queryClient = new QueryClient();
    queryClient.setQueryData(vehicleKeys.detail("123"), { id: "123", registrationPlate: "OLD" });

    const { result } = renderHook(() => useUpdateVehicle(), {
      wrapper: ({ children }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      ),
    });

    result.current.mutate({ id: "123", data: updateData });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockVehiclesService.update).toHaveBeenCalledWith(
      "123",
      updateData
    );
  });
});

describe("useDeleteVehicle", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("should delete a vehicle and invalidate queries", async () => {
    mockVehiclesService.delete.mockResolvedValueOnce(true);

    const queryClient = new QueryClient();
    queryClient.setQueryData(vehicleKeys.lists(), {
      items: [{ id: "123", registrationPlate: "ABC-001" }],
      totalCount: 1,
    });

    const { result } = renderHook(() => useDeleteVehicle(), {
      wrapper: ({ children }) => (
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      ),
    });

    result.current.mutate({
      id: "123",
      registrationPlate: "ABC-001",
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    expect(mockVehiclesService.delete).toHaveBeenCalledWith("123");
  });
});
