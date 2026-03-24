import { graphqlRequest } from "@/lib/graphql";
import { routeStatusToGraphQL } from "@/graphql/enum-maps";
import {
  MUTATION_CREATE_ROUTE,
  PAGINATED_ROUTES,
  ROUTE_BY_ID,
  VEHICLE_ROUTE_HISTORY,
} from "@/graphql/operations";
import {
  Route,
  CreateRouteRequest,
  PaginatedRoutesResult,
  RouteStatus,
} from "@/types/routes";
import {
  getMockRoutesByVehicle,
  getMockRouteById,
  mockRoutes,
} from "@/mocks/routes.mock";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

/** GraphQL often returns .NET enums as names; UI expects numeric RouteStatus */
const gqlStatusToRouteStatus: Record<string, RouteStatus> = {
  PLANNED: RouteStatus.Planned,
  IN_PROGRESS: RouteStatus.InProgress,
  COMPLETED: RouteStatus.Completed,
  CANCELLED: RouteStatus.Cancelled,
};

function mapRouteStatus(s: unknown): RouteStatus {
  if (typeof s === "number" && s >= 0 && s <= 3) return s as RouteStatus;
  if (typeof s === "string" && s in gqlStatusToRouteStatus) {
    return gqlStatusToRouteStatus[s];
  }
  return RouteStatus.Planned;
}

function mapRoute(raw: unknown): Route {
  const r = raw as Record<string, unknown>;
  return {
    id: String(r.id),
    vehicleId: String(r.vehicleId),
    vehiclePlate: String(r.vehiclePlate ?? ""),
    driverId: String(r.driverId ?? ""),
    driverName: String(r.driverName ?? ""),
    startDate:
      typeof r.startDate === "string"
        ? r.startDate
        : String(r.startDate ?? ""),
    endDate: r.endDate == null ? null : String(r.endDate),
    startMileage: Number(r.startMileage ?? 0),
    endMileage: Number(r.endMileage ?? 0),
    totalMileage: Number(r.totalMileage ?? 0),
    status: mapRouteStatus(r.status),
    parcelCount: Number(r.parcelCount ?? 0),
    parcelsDelivered: Number(r.parcelsDelivered ?? 0),
    createdAt:
      typeof r.createdAt === "string"
        ? r.createdAt
        : String(r.createdAt ?? ""),
  };
}

function mapRoutesPage(raw: {
  items: unknown[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}): PaginatedRoutesResult {
  return {
    ...raw,
    items: raw.items.map(mapRoute),
  };
}

function getMockRoutesPaginated(
  vehicleId?: string,
  page = 1,
  pageSize = 20,
  status?: RouteStatus
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
    status?: RouteStatus
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
      variables.status = routeStatusToGraphQL(status);
    }

    const data = await graphqlRequest<{
      routes: {
        items: unknown[];
        totalCount: number;
        page: number;
        pageSize: number;
        totalPages: number;
      };
    }>(PAGINATED_ROUTES, variables);
    return mapRoutesPage(data.routes);
  },

  getById: async (id: string): Promise<Route> => {
    if (USE_MOCK) {
      const route = getMockRouteById(id);
      if (!route) throw new Error("Route not found");
      return Promise.resolve(route);
    }

    const data = await graphqlRequest<{ route: unknown | null }>(
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

    const data = await graphqlRequest<{
      vehicleHistory: {
        items: unknown[];
        totalCount: number;
        page: number;
        pageSize: number;
        totalPages: number;
      };
    }>(VEHICLE_ROUTE_HISTORY, {
      vehicleId,
      page,
      pageSize,
    });
    return mapRoutesPage(data.vehicleHistory);
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
        status: 0,
        parcelCount: data.parcelIds.length,
        parcelsDelivered: 0,
        createdAt: new Date().toISOString(),
      };
      return Promise.resolve(newRoute);
    }

    const res = await graphqlRequest<{ createRoute: unknown }>(
      MUTATION_CREATE_ROUTE,
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
