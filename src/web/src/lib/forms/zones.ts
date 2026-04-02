import type { SelectOption } from "@/types/forms";
import type { Zone } from "@/types/zones";

export function zoneSelectOptions(
  zones: Zone[],
): SelectOption<string>[] {
  return zones.map((z) => ({ value: z.id, label: z.name }));
}
