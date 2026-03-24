import { graphqlRequest } from "@/lib/graphql";
import { DEPOTS_LIST } from "@/graphql/operations";
import type { DepotOption } from "@/types/depots";
import { mockDepots } from "@/mocks/depots.mock";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

export const depotsService = {
  getAll: async (): Promise<DepotOption[]> => {
    if (USE_MOCK) {
      return mockDepots;
    }

    const data = await graphqlRequest<{ depots: DepotOption[] }>(DEPOTS_LIST);
    return data.depots;
  },
};
