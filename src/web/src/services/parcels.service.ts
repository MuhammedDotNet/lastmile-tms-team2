import {
  PARCEL,
  PARCELS_FOR_ROUTE,
  REGISTER_PARCEL,
  REGISTERED_PARCELS,
} from "@/graphql/parcels";
import { graphqlRequest } from "@/lib/network/graphql-client";
import { downloadAuthenticatedFile, saveBlobAsFile } from "@/lib/network/download";
import type { GetParcelQuery, GetRegisteredParcelsQuery } from "@/graphql/parcels";
import type {
  LabelDownloadFormat,
  ParcelDetail,
  ParcelOption,
  RegisterParcelFormData,
  RegisteredParcelResult,
} from "@/types/parcels";
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

  getRegisteredParcels: async (): Promise<GetRegisteredParcelsQuery["registeredParcels"]> => {
    if (USE_MOCK) {
      return mockParcels;
    }

    const data = await graphqlRequest<{
      registeredParcels: GetRegisteredParcelsQuery["registeredParcels"];
    }>(REGISTERED_PARCELS);
    return data.registeredParcels;
  },

  getById: async (id: string): Promise<ParcelDetail> => {
    if (USE_MOCK) {
      const parcel = mockParcels.find((candidate) => candidate.id === id)?.detail;
      if (!parcel) {
        throw new Error("Parcel not found.");
      }

      return parcel;
    }

    const data = await graphqlRequest<{
      parcel: GetParcelQuery["parcel"];
    }>(PARCEL, { id });

    if (!data.parcel) {
      throw new Error("Parcel not found.");
    }

    return data.parcel as ParcelDetail;
  },

  register: async (form: RegisterParcelFormData): Promise<RegisteredParcelResult> => {
    if (USE_MOCK) {
      return {
        id: "40000000-0000-0000-0000-000000000099",
        trackingNumber: "LM20260329MOCK001",
        barcode: "LM20260329MOCK001",
        status: "Registered",
        serviceType: form.serviceType,
        weight: form.weight,
        weightUnit: form.weightUnit === 1 ? "KG" : "LB",
        length: form.length,
        width: form.width,
        height: form.height,
        dimensionUnit: form.dimensionUnit,
        declaredValue: form.declaredValue,
        currency: form.currency,
        description: form.description || null,
        parcelType: form.parcelType || null,
        estimatedDeliveryDate: form.estimatedDeliveryDate,
        createdAt: new Date().toISOString(),
        zoneId: "00000000-0000-0000-0000-000000000099",
        zoneName: "Mock Zone",
        depotId: form.shipperAddressId,
        depotName: "Mock Depot",
      };
    }

    const data = await graphqlRequest<{
      registerParcel: RegisteredParcelResult;
    }>(REGISTER_PARCEL, {
      input: {
        shipperAddressId: form.shipperAddressId,
        recipientAddress: {
          street1: form.recipientStreet1,
          street2: form.recipientStreet2 || null,
          city: form.recipientCity,
          state: form.recipientState,
          postalCode: form.recipientPostalCode,
          countryCode: form.recipientCountryCode,
          isResidential: form.recipientIsResidential,
          contactName: form.recipientContactName || null,
          companyName: form.recipientCompanyName || null,
          phone: form.recipientPhone || null,
          email: form.recipientEmail || null,
        },
        description: form.description || null,
        parcelType: form.parcelType || null,
        serviceType: form.serviceType,
        weight: form.weight,
        weightUnit: form.weightUnit === 1 ? "KG" : "LB",
        length: form.length,
        width: form.width,
        height: form.height,
        dimensionUnit: form.dimensionUnit === "CM" ? "CM" : "IN",
        declaredValue: form.declaredValue,
        currency: form.currency,
        estimatedDeliveryDate: form.estimatedDeliveryDate,
      },
    });
    return data.registerParcel;
  },

  downloadLabel: async (id: string, format: LabelDownloadFormat): Promise<string> => {
    if (USE_MOCK) {
      const parcel = mockParcels.find((candidate) => candidate.id === id);
      if (!parcel) {
        throw new Error("Parcel not found.");
      }

      const mockBody =
        format === "zpl"
          ? `^XA^FO40,40^A0N,40,40^FD${parcel.trackingNumber}^FS^XZ`
          : `Mock A4 label for ${parcel.trackingNumber}`;

      const blob = new Blob([mockBody], {
        type: format === "zpl" ? "text/plain;charset=utf-8" : "application/pdf",
      });

      const fileName =
        format === "zpl"
          ? `parcel-${parcel.trackingNumber}.zpl`
          : `parcel-${parcel.trackingNumber}-a4.pdf`;

      saveBlobAsFile(blob, fileName);
      return fileName;
    }

    return downloadAuthenticatedFile(
      format === "zpl"
        ? `/api/parcels/${id}/labels/4x6.zpl`
        : `/api/parcels/${id}/labels/a4.pdf`,
    );
  },

  downloadBulkLabels: async (
    parcelIds: string[],
    format: LabelDownloadFormat,
  ): Promise<string> => {
    if (parcelIds.length === 0) {
      throw new Error("Select at least one parcel.");
    }

    if (USE_MOCK) {
      const fileName =
        format === "zpl" ? "parcel-labels-4x6.zpl" : "parcel-labels-a4.pdf";
      const blob = new Blob(
        [
          format === "zpl"
            ? parcelIds
                .map((parcelId) => {
                  const parcel = mockParcels.find((candidate) => candidate.id === parcelId);
                  return `^XA^FO40,40^A0N,40,40^FD${parcel?.trackingNumber ?? parcelId}^FS^XZ`;
                })
                .join("\n")
            : `Mock A4 labels for ${parcelIds.length} parcels`,
        ],
        {
          type: format === "zpl" ? "text/plain;charset=utf-8" : "application/pdf",
        },
      );

      saveBlobAsFile(blob, fileName);
      return fileName;
    }

    return downloadAuthenticatedFile(
      format === "zpl"
        ? "/api/parcels/labels/4x6.zpl"
        : "/api/parcels/labels/a4.pdf",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({ parcelIds }),
      },
    );
  },
};
