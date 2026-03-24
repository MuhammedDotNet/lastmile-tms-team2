"use client";

import { useState } from "react";
import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import {
  Mail,
  Pencil,
  Plus,
  Search,
  UserX,
} from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { UserFormModal } from "@/components/common/UserManagement/UserFormModal";
import { useDebounce } from "@/hooks/use-debounce";
import type {
  CreateUserInput,
  UpdateUserInput,
  UserManagementUser,
  UserRole,
} from "@/types/user-management";
import { getUserManagementLookups, getUsers, createUser, updateUser, deactivateUser, sendPasswordResetEmail } from "@/services/user-management.service";
import type { UserFormSchema } from "@/lib/validations";

const PAGE_SIZE = 10;

function formatRole(role: UserRole | null) {
  switch (role) {
    case "Admin":
      return "Admin";
    case "OperationsManager":
      return "Operations Manager";
    case "Dispatcher":
      return "Dispatcher";
    case "WarehouseOperator":
      return "Warehouse Operator";
    case "Driver":
      return "Driver";
    default:
      return "Unassigned";
  }
}

type FilterStatus = "ALL" | "ACTIVE" | "INACTIVE";
type DialogState =
  | { mode: "create" }
  | { mode: "edit"; user: UserManagementUser }
  | null;

interface UserManagementClientProps {
  accessToken: string;
}

export function UserManagementClient({
  accessToken,
}: UserManagementClientProps) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState<"ALL" | UserRole>("ALL");
  const [statusFilter, setStatusFilter] = useState<FilterStatus>("ALL");
  const [depotFilter, setDepotFilter] = useState("ALL");
  const [zoneFilter, setZoneFilter] = useState("ALL");
  const [page, setPage] = useState(1);
  const [dialogState, setDialogState] = useState<DialogState>(null);
  const debouncedSearch = useDebounce(search, 300);

  const lookupsQuery = useQuery({
    queryKey: ["user-management", "lookups"],
    queryFn: () => getUserManagementLookups(accessToken),
  });

  const usersQuery = useQuery({
    queryKey: [
      "user-management",
      "users",
      debouncedSearch,
      roleFilter,
      statusFilter,
      depotFilter,
      zoneFilter,
      page,
    ],
    placeholderData: keepPreviousData,
    queryFn: () =>
      getUsers(accessToken, {
        search: debouncedSearch || undefined,
        role: roleFilter === "ALL" ? undefined : roleFilter,
        isActive:
          statusFilter === "ALL"
            ? undefined
            : statusFilter === "ACTIVE",
        depotId: depotFilter === "ALL" ? undefined : depotFilter,
        zoneId: zoneFilter === "ALL" ? undefined : zoneFilter,
        skip: (page - 1) * PAGE_SIZE,
        take: PAGE_SIZE,
      }),
  });

  const createMutation = useMutation({
    mutationFn: (input: CreateUserInput) => createUser(accessToken, input),
    onSuccess: () => {
      toast.success("User created and setup email queued.");
      setDialogState(null);
      queryClient.invalidateQueries({ queryKey: ["user-management", "users"] });
    },
    onError: (error: Error) => {
      toast.error(error.message);
    },
  });

  const updateMutation = useMutation({
    mutationFn: (input: UpdateUserInput) => updateUser(accessToken, input),
    onSuccess: () => {
      toast.success("User updated.");
      setDialogState(null);
      queryClient.invalidateQueries({ queryKey: ["user-management", "users"] });
    },
    onError: (error: Error) => {
      toast.error(error.message);
    },
  });

  const deactivateMutation = useMutation({
    mutationFn: (userId: string) => deactivateUser(accessToken, userId),
    onSuccess: () => {
      toast.success("User deactivated.");
      queryClient.invalidateQueries({ queryKey: ["user-management", "users"] });
    },
    onError: (error: Error) => {
      toast.error(error.message);
    },
  });

  const sendResetMutation = useMutation({
    mutationFn: (userId: string) => sendPasswordResetEmail(accessToken, userId),
    onSuccess: () => {
      toast.success("Password reset email queued.");
    },
    onError: (error: Error) => {
      toast.error(error.message);
    },
  });

  const totalCount = usersQuery.data?.totalCount ?? 0;
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE));
  const zoneOptions = lookupsQuery.data?.zones.filter(
    (zone) => depotFilter === "ALL" || zone.depotId === depotFilter
  );

  function handleSearchChange(value: string) {
    setSearch(value);
    setPage(1);
  }

  function handleRoleFilterChange(value: string) {
    setRoleFilter(value as "ALL" | UserRole);
    setPage(1);
  }

  function handleStatusFilterChange(value: string) {
    setStatusFilter(value as FilterStatus);
    setPage(1);
  }

  function handleDepotFilterChange(value: string) {
    setDepotFilter(value);
    setPage(1);

    if (
      zoneFilter !== "ALL" &&
      value !== "ALL" &&
      !lookupsQuery.data?.zones.some(
        (zone) => zone.id === zoneFilter && zone.depotId === value
      )
    ) {
      setZoneFilter("ALL");
    }
  }

  function handleZoneFilterChange(value: string) {
    setZoneFilter(value);
    setPage(1);
  }

  async function handleSubmit(values: UserFormSchema) {
    const normalized = {
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      email: values.email.trim(),
      phone: values.phone?.trim() || null,
      role: values.role,
      depotId: values.depotId || null,
      zoneId: values.zoneId || null,
    };

    if (dialogState?.mode === "edit") {
      await updateMutation.mutateAsync({
        id: dialogState.user.id,
        isActive: values.isActive,
        ...normalized,
      });
      return;
    }

    await createMutation.mutateAsync(normalized);
  }

  async function handleDeactivate(user: UserManagementUser) {
    if (user.isProtected) {
      return;
    }

    if (!window.confirm(`Deactivate ${user.fullName}?`)) {
      return;
    }

    await deactivateMutation.mutateAsync(user.id);
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">User Management</h1>
          <p className="text-muted-foreground">
            Create, edit, deactivate, and manage access for system users.
          </p>
        </div>
        <Button onClick={() => setDialogState({ mode: "create" })}>
          <Plus className="size-4" />
          New User
        </Button>
      </div>

      <div className="grid gap-4 rounded-2xl border border-border bg-card p-5 shadow-sm lg:grid-cols-[2fr_repeat(4,1fr)]">
        <div className="space-y-2">
          <LabelText>Search</LabelText>
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-2.5 size-4 text-muted-foreground" />
            <Input
              value={search}
              onChange={(event) => handleSearchChange(event.target.value)}
              placeholder="Search by name, email, or phone"
              className="pl-9"
            />
          </div>
        </div>

        <FilterSelect
          label="Role"
          value={roleFilter}
          onChange={handleRoleFilterChange}
          options={[
            { value: "ALL", label: "All roles" },
            ...(lookupsQuery.data?.roles.map((role) => ({
              value: role.value,
              label: role.label,
            })) ?? []),
          ]}
        />

        <FilterSelect
          label="Status"
          value={statusFilter}
          onChange={handleStatusFilterChange}
          options={[
            { value: "ALL", label: "All statuses" },
            { value: "ACTIVE", label: "Active" },
            { value: "INACTIVE", label: "Inactive" },
          ]}
        />

        <FilterSelect
          label="Depot"
          value={depotFilter}
          onChange={handleDepotFilterChange}
          options={[
            { value: "ALL", label: "All depots" },
            ...(lookupsQuery.data?.depots.map((depot) => ({
              value: depot.id,
              label: depot.name,
            })) ?? []),
          ]}
        />

        <FilterSelect
          label="Zone"
          value={zoneFilter}
          onChange={handleZoneFilterChange}
          options={[
            { value: "ALL", label: "All zones" },
            ...(zoneOptions?.map((zone) => ({
              value: zone.id,
              label: zone.name,
            })) ?? []),
          ]}
        />
      </div>

      <div className="overflow-hidden rounded-2xl border border-border bg-card shadow-sm">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-border text-sm">
            <thead className="bg-muted/50 text-left text-muted-foreground">
              <tr>
                <th className="px-4 py-3 font-medium">User</th>
                <th className="px-4 py-3 font-medium">Role</th>
                <th className="px-4 py-3 font-medium">Assignment</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium text-right">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {usersQuery.isLoading ? (
                <tr>
                  <td className="px-4 py-8 text-muted-foreground" colSpan={5}>
                    Loading users...
                  </td>
                </tr>
              ) : usersQuery.isError ? (
                <tr>
                  <td className="px-4 py-8 text-destructive" colSpan={5}>
                    {(usersQuery.error as Error).message}
                  </td>
                </tr>
              ) : usersQuery.data?.items.length ? (
                usersQuery.data.items.map((user) => (
                  <tr key={user.id} className="align-top">
                    <td className="px-4 py-4">
                      <div className="flex items-center gap-2">
                        <div className="font-medium">{user.fullName}</div>
                        {user.isProtected && (
                          <span className="inline-flex rounded-full bg-sky-100 px-2.5 py-1 text-xs font-medium text-sky-700">
                            System admin
                          </span>
                        )}
                      </div>
                      <div className="text-muted-foreground">{user.email}</div>
                      {user.phone && (
                        <div className="text-muted-foreground">{user.phone}</div>
                      )}
                    </td>
                    <td className="px-4 py-4">{formatRole(user.role)}</td>
                    <td className="px-4 py-4">
                      <div>{user.depotName ?? "No depot"}</div>
                      <div className="text-muted-foreground">
                        {user.zoneName ?? "No zone"}
                      </div>
                    </td>
                    <td className="px-4 py-4">
                      <span
                        className={
                          user.isActive
                            ? "inline-flex rounded-full bg-emerald-100 px-2.5 py-1 text-xs font-medium text-emerald-700"
                            : "inline-flex rounded-full bg-amber-100 px-2.5 py-1 text-xs font-medium text-amber-700"
                        }
                      >
                        {user.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                    <td className="px-4 py-4">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setDialogState({ mode: "edit", user })}
                          disabled={user.isProtected}
                        >
                          <Pencil className="size-4" />
                          Edit
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => sendResetMutation.mutate(user.id)}
                          disabled={user.isProtected || sendResetMutation.isPending}
                        >
                          <Mail className="size-4" />
                          Reset Email
                        </Button>
                        <Button
                          variant="destructive"
                          size="sm"
                          onClick={() => handleDeactivate(user)}
                          disabled={user.isProtected || !user.isActive || deactivateMutation.isPending}
                        >
                          <UserX className="size-4" />
                          Deactivate
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td className="px-4 py-8 text-muted-foreground" colSpan={5}>
                    No users matched the current filters.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="flex flex-col gap-3 border-t border-border px-4 py-4 text-sm md:flex-row md:items-center md:justify-between">
          <p className="text-muted-foreground">
            Showing {totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1}-
            {totalCount === 0 ? 0 : Math.min(page * PAGE_SIZE, totalCount)} of {totalCount} users
          </p>
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              onClick={() => setPage((current) => Math.max(1, current - 1))}
              disabled={page === 1}
            >
              Previous
            </Button>
            <span className="min-w-20 text-center">
              Page {page} of {totalPages}
            </span>
            <Button
              variant="outline"
              onClick={() =>
                setPage((current) => Math.min(totalPages, current + 1))
              }
              disabled={page >= totalPages}
            >
              Next
            </Button>
          </div>
        </div>
      </div>

      <UserFormModal
        isOpen={dialogState !== null}
        mode={dialogState?.mode ?? "create"}
        lookups={
          lookupsQuery.data ?? {
            roles: [],
            depots: [],
            zones: [],
          }
        }
        user={dialogState?.mode === "edit" ? dialogState.user : null}
        isSubmitting={createMutation.isPending || updateMutation.isPending}
        onClose={() => setDialogState(null)}
        onSubmit={handleSubmit}
      />
    </div>
  );
}

function LabelText({ children }: { children: React.ReactNode }) {
  return <p className="text-sm font-medium text-foreground">{children}</p>;
}

function FilterSelect({
  label,
  value,
  onChange,
  options,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  options: Array<{ value: string; label: string }>;
}) {
  return (
    <div className="space-y-2">
      <LabelText>{label}</LabelText>
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="flex h-8 w-full rounded-lg border border-border bg-background px-3 text-sm outline-none focus:border-ring focus:ring-2 focus:ring-ring/40"
      >
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
    </div>
  );
}
