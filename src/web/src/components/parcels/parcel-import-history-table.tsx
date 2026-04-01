"use client";

import { AlertCircle, Download, FolderClock } from "lucide-react";

import {
  ListDataTable,
  listDataTableBodyRowClass,
  listDataTableHeadRowClass,
  listDataTableTdClass,
  listDataTableThClass,
} from "@/components/list";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { appToast } from "@/lib/toast/app-toast";
import { cn } from "@/lib/utils";
import { useDownloadParcelImportErrors, useParcelImports } from "@/queries/parcels";

interface ParcelImportHistoryTableProps {
  selectedImportId?: string | null;
  onSelectImport?: (importId: string) => void;
  title?: string;
  description?: string;
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

function formatDateTime(value: string | null): string {
  if (!value) {
    return "-";
  }

  return new Date(value).toLocaleString();
}

function statusBadgeClass(status: string): string {
  switch (status) {
    case "Completed":
      return "bg-emerald-100 text-emerald-900 dark:bg-emerald-950/50 dark:text-emerald-200";
    case "CompletedWithErrors":
      return "bg-amber-100 text-amber-900 dark:bg-amber-950/50 dark:text-amber-200";
    case "Failed":
      return "bg-red-100 text-red-900 dark:bg-red-950/50 dark:text-red-200";
    default:
      return "bg-slate-100 text-slate-900 dark:bg-slate-900 dark:text-slate-200";
  }
}

export function ParcelImportHistoryTable({
  selectedImportId,
  onSelectImport,
  title = "Import History",
  description = "Recent parcel imports are retained so completed runs and rejected rows can be reviewed later.",
}: ParcelImportHistoryTableProps) {
  const { data = [], isLoading, error } = useParcelImports();
  const downloadErrors = useDownloadParcelImportErrors();

  async function handleDownloadErrors(importId: string) {
    try {
      await downloadErrors.mutateAsync(importId);
    } catch (downloadError) {
      appToast.errorFromUnknown(downloadError);
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2 text-base">
          <FolderClock className="h-4 w-4" />
          {title}
        </CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {error ? (
          <div
            role="alert"
            className="flex gap-3 rounded-xl border border-destructive/25 bg-destructive/5 px-4 py-3 text-sm text-destructive"
          >
            <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
            <p>{error instanceof Error ? error.message : "Could not load parcel import history."}</p>
          </div>
        ) : null}

        {!error && isLoading ? (
          <div className="rounded-xl border border-dashed border-border px-4 py-8 text-center text-sm text-muted-foreground">
            Loading import history...
          </div>
        ) : null}

        {!error && !isLoading && data.length === 0 ? (
          <div className="rounded-xl border border-dashed border-border px-4 py-8 text-center text-sm text-muted-foreground">
            No parcel imports have been uploaded yet.
          </div>
        ) : null}

        {!error && !isLoading && data.length > 0 ? (
          <ListDataTable minWidthClassName="min-w-[960px]">
            <thead>
              <tr className={listDataTableHeadRowClass}>
                <th className={listDataTableThClass}>File</th>
                <th className={listDataTableThClass}>Depot</th>
                <th className={listDataTableThClass}>Status</th>
                <th className={listDataTableThClass}>Rows</th>
                <th className={listDataTableThClass}>Started</th>
                <th className={listDataTableThClass}>Completed</th>
                <th className={listDataTableThClass}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {data.map((entry) => {
                const isSelected = selectedImportId === entry.id;

                return (
                  <tr key={entry.id} className={cn(listDataTableBodyRowClass, isSelected && "bg-muted/35")}>
                    <td className={cn(listDataTableTdClass, "font-medium")}>
                      <div className="flex flex-col gap-1">
                        <span>{entry.fileName}</span>
                        <span className="text-xs text-muted-foreground">{entry.fileFormat}</span>
                      </div>
                    </td>
                    <td className={listDataTableTdClass}>{entry.depotName ?? "-"}</td>
                    <td className={listDataTableTdClass}>
                      <span
                        className={cn(
                          "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium",
                          statusBadgeClass(entry.status),
                        )}
                      >
                        {formatStatus(entry.status)}
                      </span>
                    </td>
                    <td className={cn(listDataTableTdClass, "tabular-nums text-sm text-muted-foreground")}>
                      {entry.importedRows}/{entry.totalRows} imported
                      {entry.rejectedRows > 0 ? `, ${entry.rejectedRows} rejected` : ""}
                    </td>
                    <td className={cn(listDataTableTdClass, "tabular-nums text-sm text-muted-foreground")}>
                      {formatDateTime(entry.startedAt)}
                    </td>
                    <td className={cn(listDataTableTdClass, "tabular-nums text-sm text-muted-foreground")}>
                      {formatDateTime(entry.completedAt)}
                    </td>
                    <td className={listDataTableTdClass}>
                      <div className="flex flex-wrap gap-2">
                        <Button
                          type="button"
                          size="sm"
                          variant={isSelected ? "secondary" : "outline"}
                          onClick={() => onSelectImport?.(entry.id)}
                        >
                          Open
                        </Button>
                        {entry.rejectedRows > 0 ? (
                          <Button
                            type="button"
                            size="sm"
                            variant="outline"
                            onClick={() => handleDownloadErrors(entry.id)}
                            disabled={downloadErrors.isPending}
                          >
                            <Download className="h-3.5 w-3.5" aria-hidden />
                            Errors CSV
                          </Button>
                        ) : null}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </ListDataTable>
        ) : null}
      </CardContent>
    </Card>
  );
}
