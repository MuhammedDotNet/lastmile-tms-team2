"use client";

import { FilterListbox, type FilterListboxOption } from "@/components/ui/filter-listbox";
import type { DepotOption } from "@/types/depots";

interface DepotFilterDropdownProps {
  value: string | undefined;
  onChange: (value: string | undefined) => void;
  depots: DepotOption[] | undefined;
  isLoading?: boolean;
}

export function DepotFilterDropdown({
  value,
  onChange,
  depots,
  isLoading,
}: DepotFilterDropdownProps) {
  const options: FilterListboxOption<string>[] = [
    { value: undefined, label: "All Depots" },
    ...(depots?.map((d) => ({ value: d.id, label: d.name })) ?? []),
  ];

  const selectedLabel =
    options.find((o) => o.value === value)?.label ??
    (isLoading ? "Loading depots..." : "All Depots");

  return (
    <FilterListbox<string>
      value={value}
      onChange={onChange}
      options={options}
    />
  );
}
