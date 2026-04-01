"use client";

import { useState } from "react";
import { Package, PackagePlus } from "lucide-react";
import { useSession } from "next-auth/react";

import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import {
  ListDataTable,
  ListPageHeader,
  ListPageLoading,
  listDataTableBodyRowClass,
  listDataTableHeadRowClass,
  listDataTableTdClass,
  listDataTableThClass,
} from "@/components/list";
import { OverflowTooltipCell } from "@/components/list/overflow-tooltip-cell";
import { Button } from "@/components/ui/button";
import { getErrorMessage } from "@/lib/network/error-message";
import { cn } from "@/lib/utils";
import { useRegisteredParcels } from "@/queries/parcels";
import { ParcelImportHistoryTable } from "./parcel-import-history-table";
import { ParcelImportPanel } from "./parcel-import-panel";
import { ParcelRegistrationForm } from "./parcel-registration-form";

const SERVICE_TYPE_LABELS: Record<string, string> = {
  ECONOMY: "Economy",
  STANDARD: "Standard",
  EXPRESS: "Express",
  OVERNIGHT: "Overnight",
};

const statusBadgeClass =
  "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium bg-emerald-100 text-emerald-900 dark:bg-emerald-950/50 dark:text-emerald-200";

export default function ParcelsPage() {
  const { status: sessionStatus } = useSession();
  const { data = [], isLoading, error } = useRegisteredParcels();
  const [showForm, setShowForm] = useState(false);
  const [selectedImportId, setSelectedImportId] = useState<string | null>(null);

  if (sessionStatus === "loading" || isLoading) {
    return <ListPageLoading />;
  }

  if (error) {
    return (
      <QueryErrorAlert
        title="Could not load parcels"
        message={getErrorMessage(error)}
      />
    );
  }

  if (showForm) {
    return (
      <>
        <ListPageHeader
          title="Register Parcel"
          description="Enter sender, recipient, and parcel details to register a new shipment."
          icon={<PackagePlus strokeWidth={1.75} aria-hidden />}
          action={
            <Button variant="outline" onClick={() => setShowForm(false)}>
              Cancel
            </Button>
          }
        />
        <ParcelRegistrationForm
          onSuccess={() => {
            setShowForm(false);
          }}
        />
      </>
    );
  }

  return (
    <>
      <ListPageHeader
        title="Warehouse Intake Queue"
        description="Parcels registered and awaiting processing at the depot."
        icon={<Package strokeWidth={1.75} aria-hidden />}
        action={
          <Button onClick={() => setShowForm(true)}>
            <PackagePlus className="h-4 w-4" aria-hidden />
            Register Parcel
          </Button>
        }
      />

      <div className="space-y-8">
        <ParcelImportPanel
          selectedImportId={selectedImportId}
          onImportSelected={setSelectedImportId}
          showHistory={false}
        />

        {data.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-border p-12 text-center">
            <p className="font-medium">No parcels registered yet</p>
            <p className="mt-1 text-sm text-muted-foreground">
              Registered parcels will appear here in the warehouse intake queue.
            </p>
          </div>
        ) : (
          <ListDataTable minWidthClassName="min-w-[800px]">
            <thead>
              <tr className={listDataTableHeadRowClass}>
                <th className={listDataTableThClass}>Tracking</th>
                <th className={listDataTableThClass}>Weight</th>
                <th className={listDataTableThClass}>Type</th>
                <th className={listDataTableThClass}>Zone</th>
                <th className={listDataTableThClass}>Created</th>
                <th className={listDataTableThClass}>Status</th>
              </tr>
            </thead>
            <tbody>
              {data.map((parcel) => (
                <tr key={parcel.id} className={listDataTableBodyRowClass}>
                  <td className={cn(listDataTableTdClass, "font-mono text-xs font-medium")}>
                    <OverflowTooltipCell
                      fullText={parcel.trackingNumber}
                      className="font-medium"
                    >
                      {parcel.trackingNumber}
                    </OverflowTooltipCell>
                  </td>
                  <td className={cn(listDataTableTdClass, "tabular-nums")}>
                    {parcel.weight} {parcel.weightUnit}
                  </td>
                  <td className={cn(listDataTableTdClass, "text-muted-foreground")}>
                    <OverflowTooltipCell
                      fullText={
                        parcel.parcelType
                          ? `${parcel.parcelType} - ${SERVICE_TYPE_LABELS[parcel.serviceType] ?? parcel.serviceType}`
                          : SERVICE_TYPE_LABELS[parcel.serviceType] ?? parcel.serviceType
                      }
                    >
                      {parcel.parcelType
                        ? `${parcel.parcelType} - ${SERVICE_TYPE_LABELS[parcel.serviceType] ?? parcel.serviceType}`
                        : SERVICE_TYPE_LABELS[parcel.serviceType] ?? parcel.serviceType}
                    </OverflowTooltipCell>
                  </td>
                  <td className={cn(listDataTableTdClass, "text-muted-foreground")}>
                    {parcel.zoneName ?? "-"}
                  </td>
                  <td className={cn(listDataTableTdClass, "tabular-nums text-muted-foreground")}>
                    {new Date(parcel.createdAt).toLocaleDateString()}
                  </td>
                  <td className={cn(listDataTableTdClass, "max-w-[120px] align-middle")}>
                    <OverflowTooltipCell
                      shrinkToContent
                      fullText={parcel.status}
                      className={statusBadgeClass}
                    >
                      <span className={statusBadgeClass}>{parcel.status}</span>
                    </OverflowTooltipCell>
                  </td>
                </tr>
              ))}
            </tbody>
          </ListDataTable>
        )}

        <ParcelImportHistoryTable
          selectedImportId={selectedImportId}
          onSelectImport={setSelectedImportId}
        />
      </div>
    </>
  );
}
