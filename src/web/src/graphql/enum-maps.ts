import { RouteStatus } from "@/types/routes";
import { VehicleStatus, VehicleType } from "@/types/vehicles";

/** Hot Chocolate GraphQL enum names for .NET enums */
export const vehicleStatusToGraphQL = (s: VehicleStatus): string =>
  (["AVAILABLE", "IN_USE", "MAINTENANCE", "RETIRED"] as const)[s];

export const vehicleTypeToGraphQL = (t: VehicleType): string =>
  (["VAN", "CAR", "BIKE"] as const)[t];

export const routeStatusToGraphQL = (s: RouteStatus): string =>
  (["PLANNED", "IN_PROGRESS", "COMPLETED", "CANCELLED"] as const)[s];
