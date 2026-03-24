export enum RouteStatus {
  Planned = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
}

export interface Route {
  id: string;
  vehicleId: string;
  vehiclePlate: string;
  driverId: string;
  driverName: string;
  startDate: string;
  endDate: string | null;
  startMileage: number;
  endMileage: number;
  totalMileage: number;
  status: RouteStatus;
  parcelCount: number;
  parcelsDelivered: number;
  createdAt: string;
}

export interface CreateRouteRequest {
  vehicleId: string;
  driverId: string;
  startDate: string;
  startMileage: number;
  parcelIds: string[];
}

export interface PaginatedRoutesResult {
  items: Route[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
