import type { SelectOption } from "@/components/ui/select-dropdown";
import type { DepotOption } from "@/types/depots";

export function depotSelectOptions(
  depots: DepotOption[] | undefined,
): SelectOption<string>[] {
  if (!depots?.length) return [];
  return depots.map((d) => ({
    value: d.id,
    label: d.name,
  }));
}
