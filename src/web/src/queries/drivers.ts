import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { driversService } from "@/services/drivers.service";

export const driverKeys = {
  all: ["drivers"] as const,
  list: (depotId?: string) => [...driverKeys.all, "list", depotId ?? ""] as const,
};

export function useDrivers(depotId?: string) {
  const { status } = useSession();
  return useQuery({
    queryKey: driverKeys.list(depotId),
    queryFn: () => driversService.getAll(depotId),
    enabled: status === "authenticated",
  });
}
