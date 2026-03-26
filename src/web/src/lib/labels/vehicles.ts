import { cn } from "@/lib/utils";
import type { VehicleStatus, VehicleType } from "@/types/vehicles";

const vehicleStatusBadgeBase =
  "inline-flex max-w-full min-w-0 items-center truncate rounded-full px-2 py-0.5 text-xs font-medium";

export function vehicleStatusBadgeClass(status: string): string {
  switch (status) {
    case "AVAILABLE":
      return cn(
        vehicleStatusBadgeBase,
        "bg-blue-100 text-blue-800 dark:bg-blue-950/50 dark:text-blue-200",
      );
    case "IN_USE":
      return cn(
        vehicleStatusBadgeBase,
        "bg-green-100 text-green-800 dark:bg-green-950/50 dark:text-green-200",
      );
    case "MAINTENANCE":
      return cn(
        vehicleStatusBadgeBase,
        "bg-yellow-100 text-yellow-800 dark:bg-yellow-950/50 dark:text-yellow-200",
      );
    case "RETIRED":
      return cn(
        vehicleStatusBadgeBase,
        "bg-red-100 text-red-800 dark:bg-red-950/50 dark:text-red-200",
      );
    default:
      return vehicleStatusBadgeBase;
  }
}

export const VEHICLE_STATUS_LABELS: Record<VehicleStatus, string> = {
  AVAILABLE: "Available",
  IN_USE: "In Use",
  MAINTENANCE: "Maintenance",
  RETIRED: "Retired",
};

export const VEHICLE_TYPE_LABELS: Record<VehicleType, string> = {
  BIKE: "Bike",
  CAR: "Car",
  VAN: "Van",
};

const VEHICLE_TYPE_ORDER: VehicleType[] = ["BIKE", "CAR", "VAN"];

export const VEHICLE_STATUS_ORDER: VehicleStatus[] = [
  "AVAILABLE",
  "IN_USE",
  "MAINTENANCE",
  "RETIRED",
];

/** Options for {@link SelectDropdown} on vehicle create/edit forms. */
export const vehicleTypeSelectOptions = VEHICLE_TYPE_ORDER.map((v) => ({
  value: v,
  label: VEHICLE_TYPE_LABELS[v],
}));

export const vehicleStatusSelectOptions = VEHICLE_STATUS_ORDER.map((v) => ({
  value: v,
  label: VEHICLE_STATUS_LABELS[v],
}));
