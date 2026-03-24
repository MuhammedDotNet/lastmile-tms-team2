import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { parcelsService } from "@/services/parcels.service";

export const parcelKeys = {
  all: ["parcels"] as const,
  forRoute: () => [...parcelKeys.all, "forRoute"] as const,
};

export function useParcelsForRouteCreation() {
  const { status } = useSession();
  return useQuery({
    queryKey: parcelKeys.forRoute(),
    queryFn: () => parcelsService.getForRouteCreation(),
    enabled: status === "authenticated",
  });
}
