import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";

import { parcelsService } from "@/services/parcels.service";
import type {
  ParcelImportDetail,
  ParcelImportTemplateFormat,
  RegisterParcelFormData,
  RegisteredParcelResult,
  UploadParcelImportRequest,
  UploadParcelImportResult,
} from "@/types/parcels";

const parcelImportPollingStatuses = new Set(["Queued", "Processing"]);

export const parcelKeys = {
  all: ["parcels"] as const,
  forRoute: () => [...parcelKeys.all, "forRoute"] as const,
  registered: () => [...parcelKeys.all, "registered"] as const,
  imports: () => [...parcelKeys.all, "imports"] as const,
  importDetail: (id: string) => [...parcelKeys.imports(), "detail", id] as const,
};

export function useParcelsForRouteCreation() {
  const { status } = useSession();
  return useQuery({
    queryKey: parcelKeys.forRoute(),
    queryFn: () => parcelsService.getForRouteCreation(),
    enabled: status === "authenticated",
  });
}

export function useRegisteredParcels() {
  const { status } = useSession();
  return useQuery({
    queryKey: parcelKeys.registered(),
    queryFn: () => parcelsService.getRegisteredParcels(),
    enabled: status === "authenticated",
  });
}

export function useRegisterParcel() {
  const qc = useQueryClient();
  return useMutation<
    RegisteredParcelResult,
    Error,
    RegisterParcelFormData
  >({
    mutationFn: (form: RegisterParcelFormData) => parcelsService.register(form),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: parcelKeys.all });
    },
  });
}

export function useParcelImports() {
  const { status } = useSession();
  return useQuery({
    queryKey: parcelKeys.imports(),
    queryFn: () => parcelsService.getParcelImports(),
    enabled: status === "authenticated",
  });
}

export function useParcelImport(importId: string | null | undefined) {
  const { status } = useSession();

  return useQuery({
    queryKey: parcelKeys.importDetail(importId ?? "latest"),
    queryFn: () => parcelsService.getParcelImport(importId!),
    enabled: status === "authenticated" && !!importId,
    refetchInterval: (query) => {
      const data = query.state.data as ParcelImportDetail | null | undefined;
      return data && parcelImportPollingStatuses.has(data.status) ? 1000 : false;
    },
  });
}

export function useUploadParcelImport() {
  const qc = useQueryClient();

  return useMutation<UploadParcelImportResult, Error, UploadParcelImportRequest>({
    mutationFn: (request) => parcelsService.uploadParcelImport(request),
    onSuccess: async (result) => {
      await qc.invalidateQueries({ queryKey: parcelKeys.imports() });
      await qc.invalidateQueries({
        queryKey: parcelKeys.importDetail(result.importId),
      });
    },
  });
}

export function useDownloadParcelImportTemplate() {
  return useMutation<void, Error, ParcelImportTemplateFormat>({
    mutationFn: (format) => parcelsService.downloadParcelImportTemplate(format),
  });
}

export function useDownloadParcelImportErrors() {
  return useMutation<void, Error, string>({
    mutationFn: (importId) => parcelsService.downloadParcelImportErrors(importId),
  });
}
