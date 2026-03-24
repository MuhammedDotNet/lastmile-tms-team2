export enum VehicleType {
  Van = 0,
  Car = 1,
  Bike = 2,
}

export enum VehicleStatus {
  Available = 0,
  InUse = 1,
  Maintenance = 2,
  Retired = 3,
}

export interface Vehicle {
  id: string;
  registrationPlate: string;
  type: VehicleType;
  parcelCapacity: number;
  weightCapacity: number;
  status: VehicleStatus;
  depotId: string;
  depotName: string;
  /** All routes (any status) */
  totalRoutes?: number;
  routesCompleted?: number;
  totalMileage?: number;
  createdAt: string;
  lastModifiedAt: string | null;
}

export interface CreateVehicleRequest {
  registrationPlate: string;
  type: VehicleType;
  parcelCapacity: number;
  weightCapacity: number;
  status: VehicleStatus;
  depotId: string;
}

export interface UpdateVehicleRequest {
  registrationPlate: string;
  type: VehicleType;
  parcelCapacity: number;
  weightCapacity: number;
  status: VehicleStatus;
  depotId: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}