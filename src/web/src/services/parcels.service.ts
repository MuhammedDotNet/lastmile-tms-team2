import { getSession } from "next-auth/react";

import {
  GET_PARCEL_IMPORT,
  GET_PARCEL_IMPORTS,
  PARCELS_FOR_ROUTE,
  REGISTER_PARCEL,
  REGISTERED_PARCELS,
} from "@/graphql/parcels";
import type {
  GetParcelImportQuery,
  GetParcelImportsQuery,
  GetRegisteredParcelsQuery,
} from "@/graphql/parcels";
import { apiBaseUrl, parseApiErrorMessage } from "@/lib/network/api";
import { graphqlRequest } from "@/lib/network/graphql-client";
import { mockParcels } from "@/mocks/parcels.mock";
import type {
  ParcelImportDetail,
  ParcelImportHistoryEntry,
  ParcelImportTemplateFormat,
  ParcelOption,
  RegisterParcelFormData,
  RegisteredParcelResult,
  UploadParcelImportRequest,
  UploadParcelImportResult,
} from "@/types/parcels";

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

function buildApiUrl(path: string): string {
  return `${apiBaseUrl().replace(/\/$/, "")}${path}`;
}

function extractFileName(
  contentDisposition: string | null,
  fallbackFileName: string,
): string {
  if (!contentDisposition) {
    return fallbackFileName;
  }

  const utf8Match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i);
  if (utf8Match?.[1]) {
    return decodeURIComponent(utf8Match[1]);
  }

  const quotedMatch = contentDisposition.match(/filename="([^"]+)"/i);
  if (quotedMatch?.[1]) {
    return quotedMatch[1];
  }

  const plainMatch = contentDisposition.match(/filename=([^;]+)/i);
  if (plainMatch?.[1]) {
    return plainMatch[1].trim();
  }

  return fallbackFileName;
}

async function authenticatedRequest(
  path: string,
  init: RequestInit = {},
): Promise<Response> {
  const session = await getSession();
  const headers = new Headers(init.headers);

  if (session?.accessToken) {
    headers.set("Authorization", `Bearer ${session.accessToken}`);
  }

  const response = await fetch(buildApiUrl(path), {
    ...init,
    headers,
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error(await parseApiErrorMessage(response));
  }

  return response;
}

async function triggerDownload(
  response: Response,
  fallbackFileName: string,
): Promise<void> {
  if (typeof window === "undefined") {
    return;
  }

  const blob = await response.blob();
  const downloadUrl = window.URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = downloadUrl;
  link.download = extractFileName(
    response.headers.get("Content-Disposition"),
    fallbackFileName,
  );
  document.body.append(link);
  link.click();
  link.remove();
  window.URL.revokeObjectURL(downloadUrl);
}

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
      return [];
    }

    const data = await graphqlRequest<{
      registeredParcels: GetRegisteredParcelsQuery["registeredParcels"];
    }>(REGISTERED_PARCELS);
    return data.registeredParcels;
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
        depotId: "00000000-0000-0000-0000-000000000099",
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

  getParcelImports: async (): Promise<ParcelImportHistoryEntry[]> => {
    if (USE_MOCK) {
      return [];
    }

    const data = await graphqlRequest<{
      parcelImports: GetParcelImportsQuery["parcelImports"];
    }>(GET_PARCEL_IMPORTS);

    return data.parcelImports as ParcelImportHistoryEntry[];
  },

  getParcelImport: async (id: string): Promise<ParcelImportDetail | null> => {
    if (USE_MOCK) {
      return null;
    }

    const data = await graphqlRequest<{
      parcelImport: GetParcelImportQuery["parcelImport"];
    }>(GET_PARCEL_IMPORT, { id });

    return (data.parcelImport as ParcelImportDetail | null) ?? null;
  },

  uploadParcelImport: async (
    request: UploadParcelImportRequest,
  ): Promise<UploadParcelImportResult> => {
    if (USE_MOCK) {
      return { importId: "mock-import-1" };
    }

    const formData = new FormData();
    formData.append("shipperAddressId", request.shipperAddressId);
    formData.append("file", request.file);

    const response = await authenticatedRequest("/api/parcel-imports", {
      method: "POST",
      body: formData,
    });

    return (await response.json()) as UploadParcelImportResult;
  },

  downloadParcelImportTemplate: async (
    format: ParcelImportTemplateFormat,
  ): Promise<void> => {
    if (USE_MOCK) {
      return;
    }

    const response = await authenticatedRequest(
      `/api/parcel-imports/template.${format}`,
      { method: "GET" },
    );

    await triggerDownload(response, `parcel-import-template.${format}`);
  },

  downloadParcelImportErrors: async (id: string): Promise<void> => {
    if (USE_MOCK) {
      return;
    }

    const response = await authenticatedRequest(
      `/api/parcel-imports/${id}/errors.csv`,
      { method: "GET" },
    );

    await triggerDownload(response, "parcel-import-errors.csv");
  },
};
