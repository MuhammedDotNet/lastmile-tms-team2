import { useQuery } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { depotsService } from "@/services/depots.service";

export const depotKeys = {
  all: ["depots"] as const,
  list: () => [...depotKeys.all, "list"] as const,
};

export function useDepots() {
  const { status } = useSession();
  return useQuery({
    queryKey: depotKeys.list(),
    queryFn: () => depotsService.getAll(),
    enabled: status === "authenticated",
  });
}
