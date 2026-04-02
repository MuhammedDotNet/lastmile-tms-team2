export type { DriverStatus, DayOfWeek } from "@/graphql/generated";

export interface DriverOption {
  id: string;
  displayName: string;
}

export interface DriverAvailability {
  id: string;
  dayOfWeek: import("@/graphql/generated").DayOfWeek;
  shiftStart: string | null | undefined;
  shiftEnd: string | null | undefined;
  isAvailable: boolean;
}

export interface Driver {
  id: string;
  displayName: string;
  firstName: string;
  lastName: string;
  phone: string | null | undefined;
  email: string | null | undefined;
  licenseNumber: string;
  licenseExpiryDate: string | null | undefined;
  photoUrl: string | null | undefined;
  zoneId: string;
  depotId: string;
  status: import("@/graphql/generated").DriverStatus;
  userId: string;
  zoneName: string | null | undefined;
  depotName: string | null | undefined;
  userName: string | null | undefined;
  availabilitySchedule: DriverAvailability[];
  createdAt: string;
  updatedAt: string | null | undefined;
}

export interface CreateDriverRequest {
  firstName: string;
  lastName: string;
  phone?: string | null;
  email?: string | null;
  licenseNumber: string;
  licenseExpiryDate?: string | null;
  photoUrl?: string | null;
  zoneId: string;
  depotId: string;
  status: import("@/graphql/generated").DriverStatus;
  userId: string;
  availabilitySchedule: CreateDriverAvailabilityRequest[];
}

export interface UpdateDriverRequest {
  firstName: string;
  lastName: string;
  phone?: string | null;
  email?: string | null;
  licenseNumber: string;
  licenseExpiryDate?: string | null;
  photoUrl?: string | null;
  zoneId: string;
  depotId: string;
  status: import("@/graphql/generated").DriverStatus;
  userId: string;
  availabilitySchedule: UpdateDriverAvailabilityRequest[];
}

export interface CreateDriverAvailabilityRequest {
  dayOfWeek: import("@/graphql/generated").DayOfWeek;
  shiftStart?: string | null;
  shiftEnd?: string | null;
  isAvailable: boolean;
}

export interface UpdateDriverAvailabilityRequest {
  id?: string | null;
  dayOfWeek: import("@/graphql/generated").DayOfWeek;
  shiftStart?: string | null;
  shiftEnd?: string | null;
  isAvailable: boolean;
}
