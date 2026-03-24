import { MutationCache, QueryClient } from "@tanstack/react-query";

import { appToast } from "./app-toast";
import type { MutationToastMeta } from "./mutation-toast-meta";

export function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60 * 1000,
        refetchOnWindowFocus: false,
      },
    },
    mutationCache: new MutationCache({
      onError: (error, _variables, _context, mutation) => {
        const meta = mutation.options.meta as MutationToastMeta | undefined;
        if (meta?.skipErrorToast) return;
        appToast.errorFromUnknown(error);
      },
      onSuccess: (_data, variables, _context, mutation) => {
        const meta = mutation.options.meta as MutationToastMeta | undefined;
        const st = meta?.successToast;
        if (!st) return;
        const desc = st.describe?.(variables) ?? st.description;
        appToast.success(st.title, desc ? { description: desc } : undefined);
      },
    }),
  });
}
