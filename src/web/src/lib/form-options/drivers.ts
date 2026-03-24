import type { SelectOption } from "@/components/ui/select-dropdown";
import type { DriverOption } from "@/types/drivers";

export function driverSelectOptions(
  drivers: DriverOption[],
): SelectOption<string>[] {
  return drivers.map((d) => ({ value: d.id, label: d.displayName }));
}
