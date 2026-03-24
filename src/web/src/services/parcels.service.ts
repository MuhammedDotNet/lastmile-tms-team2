import { graphqlRequest } from "@/lib/graphql";
import { PARCELS_FOR_ROUTE } from "@/graphql/operations";
import { ParcelOption } from "@/types/parcels";
import { mockParcels } from "@/mocks/parcels.mock";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

export const parcelsService = {
  getForRouteCreation: async (): Promise<ParcelOption[]> => {
    if (USE_MOCK) {
      return mockParcels.map((p) => ({
        id: p.id,
        trackingNumber: p.trackingNumber,
        weight: p.weight,
        weightUnit: (p.weightUnit as string) === "Lb" ? 0 : 1,
      }));
    }

    const data = await graphqlRequest<{
      parcelsForRouteCreation: ParcelOption[];
    }>(PARCELS_FOR_ROUTE);
    return data.parcelsForRouteCreation;
  },
};
