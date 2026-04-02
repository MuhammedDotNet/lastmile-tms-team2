import {
  keepPreviousData,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import type { MutationToastMeta } from "@/lib/query/mutation-toast-meta";
import {
  createUser,
  deactivateUser,
  getUserManagementLookups,
  getUsers,
  getUsersForSelect,
  sendPasswordResetEmail,
  updateUser,
} from "@/services/users.service";
import type {
  CreateUserInput,
  GetUsersInput,
  UpdateUserInput,
} from "@/types/users";

export const usersKeys = {
  all: ["users"] as const,
  lookups: () => [...usersKeys.all, "lookups"] as const,
  list: (filters: GetUsersInput) => [...usersKeys.all, "list", filters] as const,
  select: () => [...usersKeys.all, "select"] as const,
};

export function useUsersForSelect() {
  const { status } = useSession();
  return useQuery({
    queryKey: usersKeys.select(),
    queryFn: () => getUsersForSelect(),
    enabled: status === "authenticated",
  });
}

export function useUsersLookups(accessToken: string, enabled = true) {
  return useQuery({
    queryKey: usersKeys.lookups(),
    queryFn: () => getUserManagementLookups(accessToken),
    enabled,
  });
}

export function useUsers(
  accessToken: string,
  filters: GetUsersInput,
  options?: { enabled?: boolean }
) {
  return useQuery({
    queryKey: usersKeys.list(filters),
    queryFn: () => getUsers(accessToken, filters),
    placeholderData: keepPreviousData,
    enabled: options?.enabled ?? true,
  });
}

export function useCreateUser(accessToken: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: CreateUserInput) => createUser(accessToken, input),
    meta: {
      successToast: {
        title: "User created",
        description: "User created and setup email queued.",
      },
    } satisfies MutationToastMeta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersKeys.all });
    },
  });
}

export function useUpdateUser(accessToken: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (input: UpdateUserInput) => updateUser(accessToken, input),
    meta: {
      successToast: {
        title: "User updated",
      },
    } satisfies MutationToastMeta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersKeys.all });
    },
  });
}

export function useDeactivateUser(accessToken: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (userId: string) => deactivateUser(accessToken, userId),
    meta: {
      successToast: {
        title: "User deactivated",
      },
    } satisfies MutationToastMeta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: usersKeys.all });
    },
  });
}

export function useSendPasswordResetEmail(accessToken: string) {
  return useMutation({
    mutationFn: (userId: string) => sendPasswordResetEmail(accessToken, userId),
    meta: {
      successToast: {
        title: "Password reset email queued",
      },
    } satisfies MutationToastMeta,
  });
}
