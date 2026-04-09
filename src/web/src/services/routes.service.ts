import {
  GET_ROUTE,
  GET_ROUTE_ASSIGNMENT_CANDIDATES,
  CREATE_ROUTE,
  PAGINATED_ROUTES,
  UPDATE_ROUTE_ASSIGNMENT,
  CANCEL_ROUTE,
} from "@/graphql/routes";
import type {
  CancelRouteMutation,
  CreateRouteMutation,
  GetRouteQuery,
  GetRouteAssignmentCandidatesQuery,
  GetRoutesQuery,
  UpdateRouteAssignmentMutation,
} from "@/graphql/routes";
import type { RouteFilterInput } from "@/graphql/generated";
import { graphqlRequest } from "@/lib/network/graphql-client";
import type {
  CancelRouteRequest,
  CreateRouteRequest,
  Route,
  RouteAssignmentCandidates,
  UpdateRouteAssignmentRequest,
} from "@/types/routes";
import {
  getMockRouteById,
  mockRoutes,
} from "@/mocks/routes.mock";
import { mockDrivers } from "@/mocks/drivers.mock";
import { mockVehicles } from "@/mocks/vehicles.mock";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

function mapRouteSummary(
  raw:
    | NonNullable<GetRoutesQuery["routes"]>[number]
    | NonNullable<CreateRouteMutation["createRoute"]>
    | NonNullable<UpdateRouteAssignmentMutation["updateRouteAssignment"]>
    | NonNullable<CancelRouteMutation["cancelRoute"]>,
): Route {
  return {
    id: raw.id,
    vehicleId: raw.vehicleId,
    vehiclePlate: raw.vehiclePlate?.trim() || "Unknown vehicle",
    driverId: raw.driverId,
    driverName: raw.driverName?.trim() || "Unknown driver",
    stagingArea: raw.stagingArea,
    startDate: raw.startDate,
    endDate: raw.endDate ?? null,
    startMileage: raw.startMileage,
    endMileage: raw.endMileage,
    totalMileage: raw.totalMileage,
    status: raw.status,
    parcelCount: raw.parcelCount,
    parcelsDelivered: raw.parcelsDelivered,
    createdAt: raw.createdAt,
    updatedAt: raw.updatedAt ?? null,
    cancellationReason:
      "cancellationReason" in raw ? raw.cancellationReason ?? null : null,
    assignmentAuditTrail: [],
  };
}

function mapRouteDetail(raw: NonNullable<GetRouteQuery["route"]>): Route {
  return {
    ...mapRouteSummary(raw),
    assignmentAuditTrail: raw.assignmentAuditTrail ?? [],
  };
}

export const routesService = {
  getAll: async (
    where?: RouteFilterInput
  ): Promise<Route[]> => {
    if (USE_MOCK) {
      let items = [...mockRoutes];
      if (where?.status?.eq !== undefined) {
        items = items.filter((r) => r.status === where.status!.eq);
      }
      if (where?.vehicleId?.eq !== undefined) {
        items = items.filter((r) => r.vehicleId === where.vehicleId!.eq);
      }
      return Promise.resolve(items);
    }

    const variables: Record<string, unknown> = {};
    if (where !== undefined) {
      variables.where = where;
    }

    const data = await graphqlRequest<GetRoutesQuery>(
      PAGINATED_ROUTES,
      variables
    );
    return data.routes.map(mapRouteSummary);
  },

  getById: async (id: string): Promise<Route> => {
    if (USE_MOCK) {
      const route = getMockRouteById(id);
      if (!route) throw new Error("Route not found");
      return Promise.resolve(route);
    }

    const data = await graphqlRequest<GetRouteQuery>(GET_ROUTE, { id });
    if (!data.route) throw new Error("Route not found");
    return mapRouteDetail(data.route);
  },

  create: async (data: CreateRouteRequest): Promise<Route> => {
    if (USE_MOCK) {
      const newRoute: Route = {
        id: `mock-${Date.now()}`,
        vehicleId: data.vehicleId,
        vehiclePlate: "Mock Vehicle",
        driverId: data.driverId,
        driverName: "Mock Driver",
        stagingArea: data.stagingArea,
        startDate: data.startDate,
        endDate: null,
        startMileage: data.startMileage,
        endMileage: 0,
        totalMileage: 0,
        status: "PLANNED",
        parcelCount: data.parcelIds.length,
        parcelsDelivered: 0,
        createdAt: new Date().toISOString(),
        updatedAt: null,
        cancellationReason: null,
        assignmentAuditTrail: [],
      };
      return Promise.resolve(newRoute);
    }

    const res = await graphqlRequest<CreateRouteMutation>(
      CREATE_ROUTE,
      {
        input: {
          vehicleId: data.vehicleId,
          driverId: data.driverId,
          stagingArea: data.stagingArea,
          startDate: data.startDate,
          startMileage: data.startMileage,
          parcelIds: data.parcelIds,
        },
      }
    );
    return mapRouteSummary(res.createRoute);
  },

  getAssignmentCandidates: async (
    serviceDate: string,
    routeId?: string,
  ): Promise<RouteAssignmentCandidates> => {
    if (USE_MOCK) {
      return Promise.resolve({
        vehicles: mockVehicles
          .filter((vehicle) => vehicle.status !== "MAINTENANCE" && vehicle.status !== "RETIRED")
          .map((vehicle) => ({
            id: vehicle.id,
            registrationPlate: vehicle.registrationPlate,
            depotId: vehicle.depotId,
            depotName: vehicle.depotName ?? null,
            parcelCapacity: vehicle.parcelCapacity,
            weightCapacity: vehicle.weightCapacity,
            status: vehicle.status,
            isCurrentAssignment:
              routeId !== undefined && getMockRouteById(routeId)?.vehicleId === vehicle.id,
          })),
        drivers: mockDrivers.map((driver) => ({
          id: driver.id,
          displayName: driver.name,
          depotId: mockVehicles[0]?.depotId ?? "",
          zoneId: "00000000-0000-0000-0000-000000000001",
          status: "ACTIVE",
          isCurrentAssignment:
            routeId !== undefined && getMockRouteById(routeId)?.driverId === driver.id,
          workloadRoutes: [],
        })),
      });
    }

    const data = await graphqlRequest<GetRouteAssignmentCandidatesQuery>(
      GET_ROUTE_ASSIGNMENT_CANDIDATES,
      {
        serviceDate,
        routeId,
      },
    );

    const payload = data.routeAssignmentCandidates;
    return {
      vehicles: payload?.vehicles ?? [],
      drivers: payload?.drivers ?? [],
    };
  },

  updateAssignment: async (
    id: string,
    data: UpdateRouteAssignmentRequest,
  ): Promise<Route> => {
    if (USE_MOCK) {
      const route = getMockRouteById(id);
      if (!route) throw new Error("Route not found");

      const driver =
        mockDrivers.find((candidate) => candidate.id === data.driverId) ?? null;
      const vehicle =
        mockVehicles.find((candidate) => candidate.id === data.vehicleId) ?? null;

      return Promise.resolve({
        ...route,
        driverId: data.driverId,
        driverName: driver?.name ?? route.driverName,
        vehicleId: data.vehicleId,
        vehiclePlate: vehicle?.registrationPlate ?? route.vehiclePlate,
        updatedAt: new Date().toISOString(),
      });
    }

    const result = await graphqlRequest<UpdateRouteAssignmentMutation>(
      UPDATE_ROUTE_ASSIGNMENT,
      {
        id,
        input: {
          vehicleId: data.vehicleId,
          driverId: data.driverId,
        },
      },
    );

    if (!result.updateRouteAssignment) {
      throw new Error("Route not found");
    }

    return mapRouteSummary(result.updateRouteAssignment);
  },

  cancel: async (
    id: string,
    data: CancelRouteRequest,
  ): Promise<Route> => {
    if (USE_MOCK) {
      const route = getMockRouteById(id);
      if (!route) {
        throw new Error("Route not found");
      }

      route.status = "CANCELLED";
      route.updatedAt = new Date().toISOString();
      route.cancellationReason = data.reason;
      return Promise.resolve({ ...route });
    }

    const result = await graphqlRequest<CancelRouteMutation>(CANCEL_ROUTE, {
      id,
      input: {
        reason: data.reason,
      },
    });

    if (!result.cancelRoute) {
      throw new Error("Route not found");
    }

    return mapRouteSummary(result.cancelRoute);
  },
};
