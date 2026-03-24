import { describe, it, expect, vi, beforeEach } from "vitest";
import { vehiclesService } from "../vehicles.service";
import { VehicleStatus } from "../../types/vehicles";

vi.mock("@/lib/graphql", () => ({
  graphqlRequest: vi.fn(),
}));

import { graphqlRequest } from "@/lib/graphql";

const mockGraphql = graphqlRequest as ReturnType<typeof vi.fn>;

describe("vehiclesService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe("getAll", () => {
    it("should fetch all vehicles with default pagination", async () => {
      const mockResponse = {
        items: [
          {
            id: "123",
            registrationPlate: "ABC-001",
            type: 0,
            parcelCapacity: 10,
            weightCapacity: 100,
            status: 0,
            depotId: "dep-1",
            depotName: "Main Depot",
            createdAt: "2024-01-01",
            lastModifiedAt: null,
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
        totalPages: 1,
      };

      mockGraphql.mockResolvedValueOnce({ vehicles: mockResponse });

      const result = await vehiclesService.getAll();

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("vehicles("),
        {
          page: 1,
          pageSize: 20,
        }
      );
      expect(result).toEqual(mockResponse);
    });

    it("should fetch vehicles with status filter", async () => {
      const mockResponse = {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
        totalPages: 0,
      };

      mockGraphql.mockResolvedValueOnce({ vehicles: mockResponse });

      await vehiclesService.getAll(1, 20, VehicleStatus.Available);

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("vehicles("),
        expect.objectContaining({
          status: "AVAILABLE",
        })
      );
    });

    it("should fetch vehicles with depot filter", async () => {
      const mockResponse = {
        items: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
        totalPages: 0,
      };

      mockGraphql.mockResolvedValueOnce({ vehicles: mockResponse });

      await vehiclesService.getAll(1, 20, undefined, "dep-123");

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("vehicles("),
        expect.objectContaining({
          depotId: "dep-123",
        })
      );
    });
  });

  describe("getById", () => {
    it("should fetch a vehicle by ID", async () => {
      const mockVehicle = {
        id: "123",
        registrationPlate: "ABC-001",
        type: 0,
        parcelCapacity: 10,
        weightCapacity: 100,
        status: 0,
        depotId: "dep-1",
        depotName: "Main Depot",
        createdAt: "2024-01-01",
        lastModifiedAt: null,
      };

      mockGraphql.mockResolvedValueOnce({ vehicle: mockVehicle });

      const result = await vehiclesService.getById("123");

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("vehicle("),
        { id: "123" }
      );
      expect(result).toEqual(mockVehicle);
    });
  });

  describe("create", () => {
    it("should create a new vehicle", async () => {
      const newVehicle = {
        registrationPlate: "XYZ-999",
        type: 1,
        parcelCapacity: 5,
        weightCapacity: 50,
        status: 0,
        depotId: "dep-1",
      };

      const createdVehicle = {
        id: "new-id",
        ...newVehicle,
        depotName: "Main Depot",
        createdAt: "2024-01-01",
        lastModifiedAt: null,
      };

      mockGraphql.mockResolvedValueOnce({ createVehicle: createdVehicle });

      const result = await vehiclesService.create(newVehicle);

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("createVehicle"),
        {
          input: {
            registrationPlate: "XYZ-999",
            type: "CAR",
            parcelCapacity: 5,
            weightCapacity: 50,
            status: "AVAILABLE",
            depotId: "dep-1",
          },
        }
      );
      expect(result).toEqual(createdVehicle);
    });
  });

  describe("update", () => {
    it("should update an existing vehicle", async () => {
      const updateData = {
        registrationPlate: "ABC-001",
        type: 0,
        parcelCapacity: 15,
        weightCapacity: 150,
        status: 1,
        depotId: "dep-1",
      };

      const updatedVehicle = {
        id: "123",
        ...updateData,
        depotName: "Main Depot",
        createdAt: "2024-01-01",
        lastModifiedAt: "2024-01-02",
      };

      mockGraphql.mockResolvedValueOnce({ updateVehicle: updatedVehicle });

      const result = await vehiclesService.update("123", updateData);

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("updateVehicle"),
        expect.objectContaining({
          id: "123",
          input: expect.objectContaining({
            registrationPlate: "ABC-001",
            status: "IN_USE",
          }),
        })
      );
      expect(result).toEqual(updatedVehicle);
    });
  });

  describe("delete", () => {
    it("should delete a vehicle", async () => {
      mockGraphql.mockResolvedValueOnce({ deleteVehicle: true });

      const result = await vehiclesService.delete("123");

      expect(mockGraphql).toHaveBeenCalledWith(
        expect.stringContaining("deleteVehicle"),
        { id: "123" }
      );
      expect(result).toBe(true);
    });
  });
});
