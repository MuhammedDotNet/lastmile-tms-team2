import { cn } from "@/lib/utils";
import { VehicleStatus, VehicleType } from "@/types/vehicles";

const vehicleStatusBadgeBase =
  "inline-flex max-w-full min-w-0 items-center truncate rounded-full px-2 py-0.5 text-xs font-medium";

export function vehicleStatusBadgeClass(status: VehicleStatus): string {
  switch (status) {
    case VehicleStatus.Available:
      return cn(
        vehicleStatusBadgeBase,
        "bg-blue-100 text-blue-800 dark:bg-blue-950/50 dark:text-blue-200",
      );
    case VehicleStatus.InUse:
      return cn(
        vehicleStatusBadgeBase,
        "bg-green-100 text-green-800 dark:bg-green-950/50 dark:text-green-200",
      );
    case VehicleStatus.Maintenance:
      return cn(
        vehicleStatusBadgeBase,
        "bg-yellow-100 text-yellow-800 dark:bg-yellow-950/50 dark:text-yellow-200",
      );
    case VehicleStatus.Retired:
      return cn(
        vehicleStatusBadgeBase,
        "bg-red-100 text-red-800 dark:bg-red-950/50 dark:text-red-200",
      );
    default:
      return vehicleStatusBadgeBase;
  }
}

export const VEHICLE_STATUS_LABELS: Record<VehicleStatus, string> = {
  [VehicleStatus.Available]: "Available",
  [VehicleStatus.InUse]: "In Use",
  [VehicleStatus.Maintenance]: "Maintenance",
  [VehicleStatus.Retired]: "Retired",
};

export const VEHICLE_TYPE_LABELS: Record<VehicleType, string> = {
  [VehicleType.Van]: "Van",
  [VehicleType.Car]: "Car",
  [VehicleType.Bike]: "Bike",
};

const VEHICLE_TYPE_ORDER: VehicleType[] = [
  VehicleType.Van,
  VehicleType.Car,
  VehicleType.Bike,
];

export const VEHICLE_STATUS_ORDER: VehicleStatus[] = [
  VehicleStatus.Available,
  VehicleStatus.InUse,
  VehicleStatus.Maintenance,
  VehicleStatus.Retired,
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
