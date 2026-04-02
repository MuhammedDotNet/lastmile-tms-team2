import type { SelectOption } from "@/types/forms";
import type { UserManagementUser } from "@/types/users";

export function userSelectOptions(
  users: UserManagementUser[],
): SelectOption<string>[] {
  return users.map((u) => ({ value: u.id, label: u.fullName }));
}
