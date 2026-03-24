"use client";

import { FilterListbox, type FilterListboxOption } from "@/components/ui/filter-listbox";
import { ROUTE_STATUS_LABELS, ROUTE_STATUS_ORDER } from "@/lib/labels/routes";
import {
  VEHICLE_STATUS_LABELS,
  VEHICLE_STATUS_ORDER,
} from "@/lib/labels/vehicles";
import { RouteStatus } from "@/types/routes";
import { VehicleStatus } from "@/types/vehicles";

const vehicleStatusFilterOptions: FilterListboxOption<VehicleStatus>[] = [
  { value: undefined, label: "All Statuses" },
  ...VEHICLE_STATUS_ORDER.map((v) => ({
    value: v,
    label: VEHICLE_STATUS_LABELS[v],
  })),
];

interface StatusFilterDropdownProps {
  value: VehicleStatus | undefined;
  onChange: (value: VehicleStatus | undefined) => void;
}

export function StatusFilterDropdown({ value, onChange }: StatusFilterDropdownProps) {
  return (
    <FilterListbox<VehicleStatus>
      value={value}
      onChange={onChange}
      options={vehicleStatusFilterOptions}
    />
  );
}

const routeStatusFilterOptions: FilterListboxOption<RouteStatus>[] = [
  { value: undefined, label: "All Statuses" },
  ...ROUTE_STATUS_ORDER.map((v) => ({ value: v, label: ROUTE_STATUS_LABELS[v] })),
];

interface RouteStatusFilterDropdownProps {
  value: RouteStatus | undefined;
  onChange: (value: RouteStatus | undefined) => void;
}

export function RouteStatusFilterDropdown({
  value,
  onChange,
}: RouteStatusFilterDropdownProps) {
  return (
    <FilterListbox<RouteStatus>
      value={value}
      onChange={onChange}
      options={routeStatusFilterOptions}
    />
  );
}
