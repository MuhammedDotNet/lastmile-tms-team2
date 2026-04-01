"use client";

import { useMemo, useState } from "react";
import { AlertCircle, Download, FileSpreadsheet, Upload } from "lucide-react";

import { SelectDropdown } from "@/components/form/select-dropdown";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { appToast } from "@/lib/toast/app-toast";
import { useDepots } from "@/queries/depots";
import {
  useDownloadParcelImportErrors,
  useDownloadParcelImportTemplate,
  useParcelImport,
  useParcelImports,
  useUploadParcelImport,
} from "@/queries/parcels";
import type { SelectOption } from "@/types/forms";
import { ParcelImportHistoryTable } from "./parcel-import-history-table";

interface ParcelImportPanelProps {
  selectedImportId?: string | null;
  onImportSelected?: (importId: string) => void;
  showHistory?: boolean;
}

function formatStatus(status: string): string {
  switch (status) {
    case "Queued":
      return "Queued";
    case "Processing":
      return "Processing";
    case "Completed":
      return "Completed";
    case "CompletedWithErrors":
      return "Completed with errors";
    case "Failed":
      return "Failed";
    default:
      return status;
  }
}

function statusAccentClass(status: string): string {
  switch (status) {
    case "Completed":
      return "text-emerald-700 dark:text-emerald-300";
    case "CompletedWithErrors":
      return "text-amber-700 dark:text-amber-300";
    case "Failed":
      return "text-destructive";
    default:
      return "text-sky-700 dark:text-sky-300";
  }
}

function formatDateTime(value: string | null): string {
  if (!value) {
    return "-";
  }

  return new Date(value).toLocaleString();
}

export function ParcelImportPanel({
  selectedImportId,
  onImportSelected,
  showHistory = true,
}: ParcelImportPanelProps) {
  const { data: depots = [], isLoading: depotsLoading } = useDepots();
  const { data: history = [] } = useParcelImports();
  const uploadParcelImport = useUploadParcelImport();
  const downloadTemplate = useDownloadParcelImportTemplate();
  const downloadErrors = useDownloadParcelImportErrors();

  const [internalSelectedImportId, setInternalSelectedImportId] = useState<string | null>(null);
  const [shipperAddressId, setShipperAddressId] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [uploadError, setUploadError] = useState<string | null>(null);

  const isControlledSelection = typeof onImportSelected === "function";
  const activeImportId = isControlledSelection
    ? (selectedImportId ?? history[0]?.id ?? null)
    : (internalSelectedImportId ?? history[0]?.id ?? null);
  const activeImport = useParcelImport(activeImportId);

  const depotOptions: SelectOption<string>[] = useMemo(
    () =>
      depots
        .filter((depot) => depot.isActive)
        .map((depot) => ({
          value: depot.addressId,
          label: depot.name,
        })),
    [depots],
  );

  const selectedDepot = depots.find((depot) => depot.addressId === shipperAddressId);
  const progress = activeImport.data
    ? Math.round(
        activeImport.data.totalRows > 0
          ? (activeImport.data.processedRows / activeImport.data.totalRows) * 100
          : 0,
      )
    : 0;

  const setActiveImportId = (importId: string) => {
    if (isControlledSelection) {
      onImportSelected?.(importId);
      return;
    }

    setInternalSelectedImportId(importId);
  };

  async function handleTemplateDownload(format: "csv" | "xlsx") {
    try {
      await downloadTemplate.mutateAsync(format);
    } catch (error) {
      appToast.errorFromUnknown(error);
    }
  }

  async function handleUpload() {
    if (!shipperAddressId) {
      setUploadError("Select a depot before uploading a file.");
      return;
    }

    if (!file) {
      setUploadError("Choose a .csv or .xlsx file to import.");
      return;
    }

    setUploadError(null);

    try {
      const result = await uploadParcelImport.mutateAsync({
        shipperAddressId,
        file,
      });

      setActiveImportId(result.importId);
      appToast.success("Import started", {
        description: `${file.name} is being processed in the background.`,
      });
    } catch (error) {
      setUploadError(error instanceof Error ? error.message : "Could not start the parcel import.");
    }
  }

  async function handleErrorDownload(importId: string) {
    try {
      await downloadErrors.mutateAsync(importId);
    } catch (error) {
      appToast.errorFromUnknown(error);
    }
  }

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2 text-base">
            <FileSpreadsheet className="h-4 w-4" />
            Bulk Parcel Import
          </CardTitle>
          <CardDescription>
            Upload a CSV or Excel template to register a large parcel intake in one run.
          </CardDescription>
        </CardHeader>
        <CardContent className="grid gap-6 lg:grid-cols-[minmax(0,22rem)_minmax(0,1fr)]">
          <div className="space-y-4">
            <div>
              <Label htmlFor="parcel-import-depot" className="mb-1.5 block">
                Select Depot
              </Label>
              {depotsLoading ? (
                <Input disabled placeholder="Loading depots..." />
              ) : (
                <SelectDropdown
                  id="parcel-import-depot"
                  options={depotOptions}
                  value={shipperAddressId}
                  onChange={setShipperAddressId}
                  placeholder="Select depot..."
                />
              )}
              {selectedDepot?.address ? (
                <p className="mt-2 rounded-xl border bg-muted/35 px-3 py-2 text-sm text-muted-foreground">
                  {selectedDepot.address.street1}
                  {selectedDepot.address.street2 ? `, ${selectedDepot.address.street2}` : ""}
                  <br />
                  {selectedDepot.address.city}
                  {selectedDepot.address.state ? `, ${selectedDepot.address.state}` : ""}{" "}
                  {selectedDepot.address.postalCode}
                  <br />
                  {selectedDepot.address.countryCode}
                </p>
              ) : null}
            </div>

            <div>
              <Label htmlFor="parcel-import-file" className="mb-1.5 block">
                Parcel Import File
              </Label>
              <Input
                id="parcel-import-file"
                type="file"
                accept=".csv,.xlsx"
                onChange={(event) => setFile(event.target.files?.[0] ?? null)}
              />
              <p className="mt-2 text-xs text-muted-foreground">
                Use the downloadable template to match the accepted column order exactly.
              </p>
            </div>

            <div className="flex flex-wrap gap-2">
              <Button
                type="button"
                variant="outline"
                onClick={() => handleTemplateDownload("csv")}
                disabled={downloadTemplate.isPending}
              >
                <Download className="h-3.5 w-3.5" aria-hidden />
                CSV Template
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={() => handleTemplateDownload("xlsx")}
                disabled={downloadTemplate.isPending}
              >
                <Download className="h-3.5 w-3.5" aria-hidden />
                Excel Template
              </Button>
            </div>

            {uploadError ? (
              <div
                role="alert"
                className="flex gap-3 rounded-xl border border-destructive/25 bg-destructive/5 px-4 py-3 text-sm text-destructive"
              >
                <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
                <p>{uploadError}</p>
              </div>
            ) : null}

            <Button
              type="button"
              className="w-full"
              onClick={handleUpload}
              disabled={uploadParcelImport.isPending}
            >
              <Upload className="h-4 w-4" aria-hidden />
              {uploadParcelImport.isPending ? "Uploading..." : "Start Import"}
            </Button>
          </div>

          <div className="space-y-4 rounded-2xl border bg-muted/15 p-4">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <p className="text-sm font-medium text-foreground">Latest import progress</p>
                <p className="text-sm text-muted-foreground">
                  Review live status, created tracking numbers, and rejected rows.
                </p>
              </div>
              {activeImport.data ? (
                <span className={`text-sm font-medium ${statusAccentClass(activeImport.data.status)}`}>
                  {formatStatus(activeImport.data.status)}
                </span>
              ) : null}
            </div>

            {activeImport.error ? (
              <div
                role="alert"
                className="flex gap-3 rounded-xl border border-destructive/25 bg-destructive/5 px-4 py-3 text-sm text-destructive"
              >
                <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
                <p>
                  {activeImport.error instanceof Error
                    ? activeImport.error.message
                    : "Could not load the selected parcel import."}
                </p>
              </div>
            ) : null}

            {!activeImport.data && !activeImport.error ? (
              <div className="rounded-xl border border-dashed border-border px-4 py-8 text-center text-sm text-muted-foreground">
                Upload a file or open a previous run from import history to inspect it here.
              </div>
            ) : null}

            {activeImport.data ? (
              <>
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <div>
                    <p className="font-medium">{activeImport.data.fileName}</p>
                    <p className="text-sm text-muted-foreground">
                      {activeImport.data.depotName ?? "Unknown depot"} - Started{" "}
                      {formatDateTime(activeImport.data.startedAt)}
                    </p>
                  </div>
                  {activeImport.data.rejectedRows > 0 ? (
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => {
                        const importId = activeImport.data?.id;
                        if (importId) {
                          void handleErrorDownload(importId);
                        }
                      }}
                      disabled={downloadErrors.isPending}
                    >
                      <Download className="h-3.5 w-3.5" aria-hidden />
                      Download Error Report
                    </Button>
                  ) : null}
                </div>

                <div className="space-y-2">
                  <div className="flex items-center justify-between text-sm">
                    <span className="text-muted-foreground">Progress</span>
                    <span className="font-medium">{progress}% complete</span>
                  </div>
                  <div className="h-2 overflow-hidden rounded-full bg-muted">
                    <div
                      role="progressbar"
                      aria-label="Parcel import progress"
                      aria-valuemin={0}
                      aria-valuemax={100}
                      aria-valuenow={progress}
                      className="h-full rounded-full bg-primary transition-[width] duration-300"
                      style={{ width: `${progress}%` }}
                    />
                  </div>
                </div>

                <div className="grid gap-3 sm:grid-cols-4">
                  <div className="rounded-xl border bg-background px-3 py-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Total rows</p>
                    <p className="mt-1 text-xl font-semibold tabular-nums">{activeImport.data.totalRows}</p>
                  </div>
                  <div className="rounded-xl border bg-background px-3 py-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Processed</p>
                    <p className="mt-1 text-xl font-semibold tabular-nums">{activeImport.data.processedRows}</p>
                  </div>
                  <div className="rounded-xl border bg-background px-3 py-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Imported</p>
                    <p className="mt-1 text-xl font-semibold tabular-nums">{activeImport.data.importedRows}</p>
                  </div>
                  <div className="rounded-xl border bg-background px-3 py-3">
                    <p className="text-xs uppercase tracking-wide text-muted-foreground">Rejected</p>
                    <p className="mt-1 text-xl font-semibold tabular-nums">{activeImport.data.rejectedRows}</p>
                  </div>
                </div>

                {activeImport.data.failureMessage ? (
                  <div className="rounded-xl border border-destructive/20 bg-destructive/5 px-4 py-3 text-sm text-destructive">
                    {activeImport.data.failureMessage}
                  </div>
                ) : null}

                {activeImport.data.createdTrackingNumbers.length > 0 ? (
                  <div className="space-y-2">
                    <p className="text-sm font-medium">Created tracking numbers</p>
                    <div className="flex flex-wrap gap-2">
                      {activeImport.data.createdTrackingNumbers.map((trackingNumber) => (
                        <span
                          key={trackingNumber}
                          className="rounded-full border bg-background px-3 py-1 font-mono text-xs"
                        >
                          {trackingNumber}
                        </span>
                      ))}
                    </div>
                  </div>
                ) : null}

                {activeImport.data.rowFailuresPreview.length > 0 ? (
                  <div className="space-y-2">
                    <p className="text-sm font-medium">Rejected rows</p>
                    <div className="space-y-2">
                      {activeImport.data.rowFailuresPreview.map((rowFailure) => (
                        <div
                          key={`${rowFailure.rowNumber}-${rowFailure.errorMessage}`}
                          className="rounded-xl border bg-background px-4 py-3"
                        >
                          <p className="text-sm font-medium">
                            Row {rowFailure.rowNumber}
                          </p>
                          <p className="mt-1 text-sm text-muted-foreground">
                            {rowFailure.errorMessage}
                          </p>
                        </div>
                      ))}
                    </div>
                  </div>
                ) : null}
              </>
            ) : null}
          </div>
        </CardContent>
      </Card>

      {showHistory ? (
        <ParcelImportHistoryTable
          selectedImportId={activeImportId}
          onSelectImport={setActiveImportId}
        />
      ) : null}
    </div>
  );
}
