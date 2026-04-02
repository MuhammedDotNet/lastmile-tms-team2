import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import type { MutationToastMeta } from "@/lib/query/mutation-toast-meta";
import { driversService } from "@/services/drivers.service";
import {
  CreateDriverRequest,
  UpdateDriverRequest,
} from "@/types/drivers";
import type { DriverFilterInput, DriverStatus } from "@/graphql/generated";

export const driverKeys = {
  all: ["drivers"] as const,
  lists: () => [...driverKeys.all, "list"] as const,
  list: (where?: DriverFilterInput) =>
    [...driverKeys.lists(), where] as const,
  details: () => [...driverKeys.all, "detail"] as const,
  detail: (id: string) => [...driverKeys.details(), id] as const,
};

export function useDrivers(params?: string | { status?: DriverStatus; depotId?: string }) {
  const { status } = useSession();

  let depotId: string | undefined;
  let driverStatus: DriverStatus | undefined;

  if (typeof params === "string") {
    depotId = params;
  } else if (params) {
    depotId = params.depotId;
    driverStatus = params.status;
  }

  const where: DriverFilterInput | undefined =
    driverStatus !== undefined || depotId !== undefined
      ? {
          ...(driverStatus !== undefined && {
            status: { eq: driverStatus },
          }),
          ...(depotId !== undefined && {
            depotId: { eq: depotId },
          }),
        }
      : undefined;

  return useQuery({
    queryKey: driverKeys.list(where),
    queryFn: () => driversService.getAll(where),
    enabled: status === "authenticated",
  });
}

export function useDriver(id: string) {
  const { status } = useSession();
  return useQuery({
    queryKey: driverKeys.detail(id),
    queryFn: () => driversService.getById(id),
    enabled: status === "authenticated" && !!id,
  });
}

export function useCreateDriver() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateDriverRequest) => driversService.create(data),
    meta: {
      successToast: {
        title: "Driver created",
        describe: (variables) => {
          const v = variables as CreateDriverRequest;
          return `${v.firstName} ${v.lastName} was added.`;
        },
      },
    } satisfies MutationToastMeta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
    },
  });
}

export function useUpdateDriver() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateDriverRequest;
    }) => driversService.update(id, data),
    meta: {
      successToast: {
        title: "Driver updated",
        describe: (variables) => {
          const v = variables as { data: UpdateDriverRequest };
          return `${v.data.firstName} ${v.data.lastName} was saved successfully.`;
        },
      },
    } satisfies MutationToastMeta,
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
      queryClient.invalidateQueries({ queryKey: driverKeys.detail(id) });
    },
  });
}

export function useDeleteDriver() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (vars: { id: string; displayName?: string }) =>
      driversService.delete(vars.id),
    meta: {
      successToast: {
        title: "Driver deleted",
        describe: (variables) => {
          const v = variables as { displayName?: string };
          return v.displayName
            ? `${v.displayName} was removed.`
            : undefined;
        },
      },
    } satisfies MutationToastMeta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: driverKeys.all });
    },
  });
}
