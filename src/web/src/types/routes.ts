export type { RouteStatus, StagingArea } from "@/graphql/generated";

export type Route = {
  id: string;
  vehicleId: string;
  vehiclePlate: string;
  driverId: string;
  driverName: string;
  stagingArea: import("@/graphql/generated").StagingArea;
  startDate: string;
  endDate: string | null;
  startMileage: number;
  endMileage: number;
  totalMileage: number;
  status: import("@/graphql/generated").RouteStatus;
  parcelCount: number;
  parcelsDelivered: number;
  createdAt: string;
};

export type CreateRouteRequest = {
  vehicleId: string;
  driverId: string;
  stagingArea: import("@/graphql/generated").StagingArea;
  startDate: string;
  startMileage: number;
  parcelIds: string[];
};
