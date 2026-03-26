import {
  CREATE_ROUTE,
  PAGINATED_ROUTES,
  ROUTE_BY_ID,
  VEHICLE_ROUTE_HISTORY,
} from "@/graphql/routes";
import type {
  GetRoutesQuery,
  GetRouteQuery,
  GetVehicleHistoryQuery,
  CreateRouteMutation,
} from "@/graphql/routes";
import { graphqlRequest } from "@/lib/network/graphql-client";
import type {
  Route,
  CreateRouteRequest,
  PaginatedRoutesResult,
} from "@/types/routes";
import {
  getMockRoutesByVehicle,
  getMockRouteById,
  mockRoutes,
} from "@/mocks/routes.mock";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

function mapRoute(raw: NonNullable<GetRouteQuery["route"]>): Route {
  return {
    id: raw.id,
    vehicleId: raw.vehicleId,
    vehiclePlate: raw.vehiclePlate,
    driverId: raw.driverId,
    driverName: raw.driverName,
    startDate: raw.startDate,
    endDate: raw.endDate ?? null,
    startMileage: raw.startMileage,
    endMileage: raw.endMileage,
    totalMileage: raw.totalMileage,
    status: raw.status,
    parcelCount: raw.parcelCount,
    parcelsDelivered: raw.parcelsDelivered,
    createdAt: raw.createdAt,
  };
}

function getMockRoutesPaginated(
  vehicleId?: string,
  page = 1,
  pageSize = 20,
  status?: string
): PaginatedRoutesResult {
  let items = vehicleId
    ? getMockRoutesByVehicle(vehicleId)
    : [...mockRoutes];
  if (status !== undefined) {
    items = items.filter((r) => r.status === status);
  }

  const totalCount = items.length;
  const totalPages = Math.ceil(totalCount / pageSize) || 1;
  const start = (page - 1) * pageSize;
  const paginatedItems = items.slice(start, start + pageSize);

  return {
    items: paginatedItems,
    totalCount,
    page,
    pageSize,
    totalPages,
  };
}

export const routesService = {
  getAll: async (
    vehicleId?: string,
    page = 1,
    pageSize = 20,
    status?: string
  ): Promise<PaginatedRoutesResult> => {
    if (USE_MOCK) {
      return Promise.resolve(
        getMockRoutesPaginated(vehicleId, page, pageSize, status)
      );
    }

    const variables: Record<string, unknown> = { page, pageSize };
    if (vehicleId !== undefined && vehicleId.trim() !== "") {
      variables.vehicleId = vehicleId;
    }
    if (status !== undefined) {
      variables.status = status;
    }

    const data = await graphqlRequest<GetRoutesQuery>(PAGINATED_ROUTES, variables);
    const p = data.routes;
    return {
      ...p,
      items: p.items.map((item) => ({
        id: item.id,
        vehicleId: item.vehicleId,
        vehiclePlate: item.vehiclePlate,
        driverId: item.driverId,
        driverName: item.driverName,
        startDate: item.startDate,
        endDate: item.endDate ?? null,
        startMileage: item.startMileage,
        endMileage: item.endMileage,
        totalMileage: item.totalMileage,
        status: item.status,
        parcelCount: item.parcelCount,
        parcelsDelivered: item.parcelsDelivered,
        createdAt: item.createdAt,
      })),
    };
  },

  getById: async (id: string): Promise<Route> => {
    if (USE_MOCK) {
      const route = getMockRouteById(id);
      if (!route) throw new Error("Route not found");
      return Promise.resolve(route);
    }

    const data = await graphqlRequest<GetRouteQuery>(
      ROUTE_BY_ID,
      { id }
    );
    if (!data.route) throw new Error("Route not found");
    return mapRoute(data.route);
  },

  getVehicleHistory: async (
    vehicleId: string,
    page = 1,
    pageSize = 10
  ): Promise<PaginatedRoutesResult> => {
    if (USE_MOCK) {
      return Promise.resolve(
        getMockRoutesPaginated(vehicleId, page, pageSize)
      );
    }

    const data = await graphqlRequest<GetVehicleHistoryQuery>(
      VEHICLE_ROUTE_HISTORY,
      { vehicleId, page, pageSize }
    );
    const p = data.vehicleHistory;
    return {
      ...p,
      items: p.items.map((item) => ({
        id: item.id,
        vehicleId: item.vehicleId,
        vehiclePlate: item.vehiclePlate,
        driverId: item.driverId,
        driverName: item.driverName,
        startDate: item.startDate,
        endDate: item.endDate ?? null,
        startMileage: item.startMileage,
        endMileage: item.endMileage,
        totalMileage: item.totalMileage,
        status: item.status,
        parcelCount: item.parcelCount,
        parcelsDelivered: item.parcelsDelivered,
        createdAt: item.createdAt,
      })),
    };
  },

  create: async (data: CreateRouteRequest): Promise<Route> => {
    if (USE_MOCK) {
      const newRoute: Route = {
        id: `mock-${Date.now()}`,
        vehicleId: data.vehicleId,
        vehiclePlate: "Mock Vehicle",
        driverId: data.driverId,
        driverName: "Mock Driver",
        startDate: data.startDate,
        endDate: null,
        startMileage: data.startMileage,
        endMileage: 0,
        totalMileage: 0,
        status: "PLANNED",
        parcelCount: data.parcelIds.length,
        parcelsDelivered: 0,
        createdAt: new Date().toISOString(),
      };
      return Promise.resolve(newRoute);
    }

    const res = await graphqlRequest<CreateRouteMutation>(
      CREATE_ROUTE,
      {
        input: {
          vehicleId: data.vehicleId,
          driverId: data.driverId,
          startDate: data.startDate,
          startMileage: data.startMileage,
          parcelIds: data.parcelIds,
        },
      }
    );
    return mapRoute(res.createRoute);
  },
};
