import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import type { MutationToastMeta } from "@/lib/mutation-toast-meta";
import { routesService } from "@/services/routes.service";
import { CreateRouteRequest, RouteStatus } from "@/types/routes";
import { vehicleKeys } from "./vehicles";
import { parcelKeys } from "./parcels";

export const routeKeys = {
  all: ["routes"] as const,
  lists: () => [...routeKeys.all, "list"] as const,
  list: (params: {
    vehicleId?: string;
    page?: number;
    pageSize?: number;
    status?: RouteStatus;
  }) => [...routeKeys.lists(), params] as const,
  details: () => [...routeKeys.all, "detail"] as const,
  detail: (id: string) => [...routeKeys.details(), id] as const,
  vehicleHistory: (vehicleId: string, page?: number) =>
    [...routeKeys.all, "vehicleHistory", vehicleId, page] as const,
};

export function useRoutes(params: {
  vehicleId?: string;
  page?: number;
  pageSize?: number;
  status?: RouteStatus;
}) {
  const { status } = useSession();
  return useQuery({
    queryKey: routeKeys.list(params),
    queryFn: () =>
      routesService.getAll(
        params.vehicleId,
        params.page,
        params.pageSize,
        params.status
      ),
    enabled: status === "authenticated",
  });
}

export function useRoute(id: string) {
  const { status } = useSession();
  return useQuery({
    queryKey: routeKeys.detail(id),
    queryFn: () => routesService.getById(id),
    enabled: status === "authenticated" && !!id,
  });
}

export function useVehicleHistory(vehicleId: string, page = 1) {
  const { status } = useSession();
  return useQuery({
    queryKey: routeKeys.vehicleHistory(vehicleId, page),
    queryFn: () => routesService.getVehicleHistory(vehicleId, page, 10),
    enabled: status === "authenticated" && !!vehicleId,
  });
}

export function useCreateRoute() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateRouteRequest) => routesService.create(data),
    meta: {
      successToast: {
        title: "Route created",
        description: "The route appears in the list and is ready for dispatch.",
      },
    } satisfies MutationToastMeta,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: routeKeys.all });
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all });
      queryClient.invalidateQueries({ queryKey: parcelKeys.all });
    },
  });
}
