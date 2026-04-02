"use client";

import { FilterListbox, type FilterListboxOption } from "@/components/form/filter-listbox";
import { DRIVER_STATUS_SELECT_OPTIONS } from "@/lib/labels/drivers";
import type { DriverStatus } from "@/types/drivers";

const allStatusesOption: FilterListboxOption<DriverStatus | undefined> = {
  value: undefined,
  label: "All statuses",
};

const driverStatusOptions: FilterListboxOption<DriverStatus | undefined>[] =
  DRIVER_STATUS_SELECT_OPTIONS.map((opt) => ({
    value: opt.value as DriverStatus,
    label: opt.label,
  }));

interface DriverStatusFilterProps {
  value: DriverStatus | undefined;
  onChange: (value: DriverStatus | undefined) => void;
}

export function DriverStatusFilter({
  value,
  onChange,
}: DriverStatusFilterProps) {
  return (
    <FilterListbox<DriverStatus | undefined>
      value={value}
      onChange={onChange}
      options={[allStatusesOption, ...driverStatusOptions]}
    />
  );
}
