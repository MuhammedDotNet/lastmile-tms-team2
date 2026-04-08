import { beforeEach, describe, expect, it, vi } from "vitest";
import { binLocationsService } from "../bin-locations.service";

vi.mock("@/lib/network/graphql-client", () => ({
  graphqlRequest: vi.fn(),
}));

import { graphqlRequest } from "@/lib/network/graphql-client";

const mockGraphql = graphqlRequest as ReturnType<typeof vi.fn>;

describe("binLocationsService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("normalizes depot storage layout responses", async () => {
    mockGraphql.mockResolvedValueOnce({
      depotStorageLayout: {
        depotId: "depot-1",
        depotName: "North Depot",
        availableDeliveryZones: [
          {
            id: "delivery-zone-1",
            name: "Metro North",
          },
        ],
        storageZones: [
          {
            id: "zone-1",
            name: "Storage Zone A",
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
                ],
              },
            ],
          },
        ],
      },
    });

    const result = await binLocationsService.getDepotStorageLayout("depot-1");

    expect(result?.depotName).toBe("North Depot");
    expect(result?.availableDeliveryZones).toEqual([
      {
        id: "delivery-zone-1",
        name: "Metro North",
      },
    ]);
    expect(result?.storageZones).toHaveLength(1);
    expect(result?.storageZones[0].storageAisles[0].binLocations[0].name).toBe(
      "BIN-01",
    );
    expect(
      result?.storageZones[0].storageAisles[0].binLocations[0].deliveryZoneName,
    ).toBe("Metro North");
  });

  it("submits storage zone create payloads", async () => {
    mockGraphql.mockResolvedValueOnce({
      createStorageZone: {
        id: "zone-2",
        name: "Storage Zone B",
        depotId: "depot-1",
        storageAisles: [],
      },
    });

    await binLocationsService.createStorageZone({
      depotId: "depot-1",
      name: "Storage Zone B",
    });

    expect(mockGraphql).toHaveBeenCalledWith(expect.any(Object), {
      input: {
        depotId: "depot-1",
        name: "Storage Zone B",
      },
    });
  });

  it("submits bin updates with active state", async () => {
    mockGraphql.mockResolvedValueOnce({
      updateBinLocation: {
        id: "bin-1",
        name: "BIN-02",
        isActive: false,
        storageAisleId: "aisle-1",
        deliveryZoneId: null,
        deliveryZoneName: null,
      },
    });

    await binLocationsService.updateBinLocation("bin-1", {
      name: "BIN-02",
      isActive: false,
    });

    expect(mockGraphql).toHaveBeenCalledWith(expect.any(Object), {
      id: "bin-1",
      input: {
        name: "BIN-02",
        isActive: false,
      },
    });
  });

  it("submits bin create payloads with delivery zone assignments", async () => {
    mockGraphql.mockResolvedValueOnce({
      createBinLocation: {
        id: "bin-2",
        name: "BIN-02",
        isActive: true,
        storageAisleId: "aisle-1",
        deliveryZoneId: "delivery-zone-1",
        deliveryZoneName: "Metro North",
      },
    });

    await binLocationsService.createBinLocation({
      storageAisleId: "aisle-1",
      name: "BIN-02",
      isActive: true,
      deliveryZoneId: "delivery-zone-1",
    });

    expect(mockGraphql).toHaveBeenCalledWith(expect.any(Object), {
      input: {
        storageAisleId: "aisle-1",
        name: "BIN-02",
        isActive: true,
        deliveryZoneId: "delivery-zone-1",
      },
    });
  });

  it("omits bin active state when no status change is requested", async () => {
    mockGraphql.mockResolvedValueOnce({
      updateBinLocation: {
        id: "bin-1",
        name: "BIN-03",
        isActive: true,
        storageAisleId: "aisle-1",
        deliveryZoneId: "delivery-zone-1",
        deliveryZoneName: "Metro North",
      },
    });

    await binLocationsService.updateBinLocation("bin-1", {
      name: "BIN-03",
    });

    expect(mockGraphql).toHaveBeenCalledWith(expect.any(Object), {
      id: "bin-1",
      input: {
        name: "BIN-03",
      },
    });
  });

  it("sends null delivery zone when clearing a bin assignment", async () => {
    mockGraphql.mockResolvedValueOnce({
      updateBinLocation: {
        id: "bin-1",
        name: "BIN-03",
        isActive: true,
        storageAisleId: "aisle-1",
        deliveryZoneId: null,
        deliveryZoneName: null,
      },
    });

    await binLocationsService.updateBinLocation("bin-1", {
      name: "BIN-03",
      deliveryZoneId: null,
    });

    expect(mockGraphql).toHaveBeenCalledWith(expect.any(Object), {
      id: "bin-1",
      input: {
        name: "BIN-03",
        deliveryZoneId: null,
      },
    });
  });

  it("submits delete aisle payloads", async () => {
    mockGraphql.mockResolvedValueOnce({
      deleteStorageAisle: true,
    });

    await binLocationsService.deleteStorageAisle("aisle-1");

    expect(mockGraphql).toHaveBeenCalledWith(expect.any(Object), {
      id: "aisle-1",
    });
  });
});
