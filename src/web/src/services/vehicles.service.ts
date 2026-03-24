import { graphqlRequest } from "@/lib/graphql";
import {
  vehicleStatusToGraphQL,
  vehicleTypeToGraphQL,
} from "@/graphql/enum-maps";
import {
  MUTATION_CREATE_VEHICLE,
  MUTATION_DELETE_VEHICLE,
  MUTATION_UPDATE_VEHICLE,
  PAGINATED_VEHICLES,
  VEHICLE_BY_ID,
} from "@/graphql/operations";
import {
  Vehicle,
  CreateVehicleRequest,
  UpdateVehicleRequest,
  PaginatedResponse,
  VehicleStatus,
  VehicleType,
} from "@/types/vehicles";
import {
  getMockVehiclesPaginated,
  getMockVehicleById,
} from "@/mocks/vehicles.mock";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

const gqlStatusToVehicleStatus: Record<string, VehicleStatus> = {
  AVAILABLE: VehicleStatus.Available,
  IN_USE: VehicleStatus.InUse,
  MAINTENANCE: VehicleStatus.Maintenance,
  RETIRED: VehicleStatus.Retired,
};

const gqlTypeToVehicleType: Record<string, VehicleType> = {
  VAN: VehicleType.Van,
  CAR: VehicleType.Car,
  BIKE: VehicleType.Bike,
};

function mapVehicleStatus(v: unknown): VehicleStatus {
  if (typeof v === "number" && v >= 0 && v <= 3) return v as VehicleStatus;
  if (typeof v === "string" && v in gqlStatusToVehicleStatus) {
    return gqlStatusToVehicleStatus[v];
  }
  return VehicleStatus.Available;
}

function mapVehicleType(t: unknown): VehicleType {
  if (typeof t === "number" && t >= 0 && t <= 2) return t as VehicleType;
  if (typeof t === "string" && t in gqlTypeToVehicleType) {
    return gqlTypeToVehicleType[t];
  }
  return VehicleType.Van;
}

/** GraphQL/JSON may use camelCase or PascalCase depending on serializer. */
function pickNumber(
  v: Record<string, unknown>,
  camel: string,
  pascal: string
): number | undefined {
  const raw = v[camel] ?? v[pascal];
  if (raw === null || raw === undefined || raw === "") return undefined;
  const n = Number(raw);
  return Number.isFinite(n) ? n : undefined;
}

function mapVehicle(raw: unknown): Vehicle {
  const v = raw as Record<string, unknown>;
  return {
    id: String(v.id),
    registrationPlate: String(v.registrationPlate ?? ""),
    type: mapVehicleType(v.type),
    parcelCapacity: Number(v.parcelCapacity ?? 0),
    weightCapacity: Number(v.weightCapacity ?? 0),
    status: mapVehicleStatus(v.status),
    depotId: String(v.depotId ?? ""),
    depotName: String(v.depotName ?? ""),
    totalRoutes: pickNumber(v, "totalRoutes", "TotalRoutes"),
    routesCompleted: pickNumber(v, "routesCompleted", "RoutesCompleted"),
    totalMileage: pickNumber(v, "totalMileage", "TotalMileage"),
    createdAt:
      typeof v.createdAt === "string"
        ? v.createdAt
        : String(v.createdAt ?? ""),
    lastModifiedAt:
      v.lastModifiedAt == null
        ? null
        : typeof v.lastModifiedAt === "string"
          ? v.lastModifiedAt
          : String(v.lastModifiedAt),
  };
}

export const vehiclesService = {
  getAll: async (
    page = 1,
    pageSize = 20,
    status?: VehicleStatus,
    depotId?: string
  ): Promise<PaginatedResponse<Vehicle>> => {
    if (USE_MOCK) {
      return Promise.resolve(
        getMockVehiclesPaginated(page, pageSize, status)
      );
    }

    const variables: Record<string, unknown> = { page, pageSize };
    if (status !== undefined) {
      variables.status = vehicleStatusToGraphQL(status);
    }
    if (depotId !== undefined && depotId.trim() !== "") {
      variables.depotId = depotId;
    }

    const data = await graphqlRequest<{
      vehicles: PaginatedResponse<unknown>;
    }>(PAGINATED_VEHICLES, variables);
    const p = data.vehicles;
    return {
      ...p,
      items: p.items.map(mapVehicle),
    };
  },

  getById: async (id: string): Promise<Vehicle> => {
    if (USE_MOCK) {
      const vehicle = getMockVehicleById(id);
      if (!vehicle) throw new Error("Vehicle not found");
      return Promise.resolve(vehicle);
    }

    const data = await graphqlRequest<{ vehicle: unknown | null }>(
      VEHICLE_BY_ID,
      { id }
    );
    if (!data.vehicle) throw new Error("Vehicle not found");
    return mapVehicle(data.vehicle);
  },

  create: async (data: CreateVehicleRequest): Promise<Vehicle> => {
    if (USE_MOCK) {
      const newVehicle: Vehicle = {
        ...data,
        id: `mock-${Date.now()}`,
        depotName: "Test Depot",
        createdAt: new Date().toISOString(),
        lastModifiedAt: null,
      };
      return Promise.resolve(newVehicle);
    }

    const res = await graphqlRequest<{ createVehicle: unknown }>(
      MUTATION_CREATE_VEHICLE,
      {
        input: {
          registrationPlate: data.registrationPlate,
          type: vehicleTypeToGraphQL(data.type),
          parcelCapacity: data.parcelCapacity,
          weightCapacity: data.weightCapacity,
          status: vehicleStatusToGraphQL(data.status),
          depotId: data.depotId,
        },
      }
    );
    return mapVehicle(res.createVehicle);
  },

  update: async (id: string, data: UpdateVehicleRequest): Promise<Vehicle> => {
    if (USE_MOCK) {
      const existing = getMockVehicleById(id);
      if (!existing) throw new Error("Vehicle not found");
      const updated: Vehicle = {
        ...existing,
        ...data,
        depotName: existing.depotName,
        lastModifiedAt: new Date().toISOString(),
      };
      return Promise.resolve(updated);
    }

    const res = await graphqlRequest<{ updateVehicle: unknown | null }>(
      MUTATION_UPDATE_VEHICLE,
      {
        id,
        input: {
          registrationPlate: data.registrationPlate,
          type: vehicleTypeToGraphQL(data.type),
          parcelCapacity: data.parcelCapacity,
          weightCapacity: data.weightCapacity,
          status: vehicleStatusToGraphQL(data.status),
          depotId: data.depotId,
        },
      }
    );
    if (!res.updateVehicle) throw new Error("Vehicle not found");
    return mapVehicle(res.updateVehicle);
  },

  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      if (!getMockVehicleById(id)) throw new Error("Vehicle not found");
      return Promise.resolve(true);
    }

    const res = await graphqlRequest<{ deleteVehicle: boolean }>(
      MUTATION_DELETE_VEHICLE,
      { id }
    );
    return res.deleteVehicle;
  },
};
