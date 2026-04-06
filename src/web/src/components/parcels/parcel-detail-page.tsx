"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import {
  ArrowLeft,
  CalendarDays,
  MapPin,
  Package,
  Pencil,
  Printer,
  Route,
  Truck,
  UserRound,
} from "lucide-react";
import { useSession } from "next-auth/react";

import {
  DetailBreadcrumb,
  DetailContainer,
  DetailEmptyState,
  DetailField,
  DetailFieldGrid,
  DetailHero,
  DetailMetricStrip,
  DetailPageSkeleton,
  DetailPanel,
  DetailShell,
  DETAIL_PAGE_CONTENT_PADDING,
} from "@/components/detail";
import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import { CancelParcelDialog } from "@/components/parcels/cancel-parcel-dialog";
import { buttonVariants } from "@/components/ui/button";
import {
  formatParcelServiceType,
  formatParcelStatus,
  parcelStatusBadgeClass,
} from "@/lib/labels/parcels";
import {
  getParcelDetailPath,
  getParcelEditPath,
} from "@/lib/parcels/paths";
import { getErrorMessage } from "@/lib/network/error-message";
import { appToast } from "@/lib/toast/app-toast";
import { cn } from "@/lib/utils";
import { isGuidString } from "@/lib/validation/guid-string";
import {
  useCancelParcel,
  useDownloadParcelLabel,
  useParcel,
  useParcelRealtimeUpdates,
} from "@/queries/parcels";
import type {
  LabelDownloadFormat,
  ParcelDetailAddress,
} from "@/types/parcels";
import { ParcelStatusTransition } from "./parcel-status-transition";
import { ParcelTimeline } from "./parcel-timeline";

function formatAddressLineTwo(
  city: string,
  state: string,
  postalCode: string,
  countryCode: string,
) {
  return [city, [state, postalCode].filter(Boolean).join(" "), countryCode]
    .filter(Boolean)
    .join(", ");
}

function formatHistoryValue(value: string | null) {
  return value && value.trim() ? value : "-";
}

function formatAddress(address: ParcelDetailAddress) {
  return (
    <div className="space-y-1">
      <p>{address.street1}</p>
      {address.street2 ? <p>{address.street2}</p> : null}
      <p>
        {formatAddressLineTwo(
          address.city,
          address.state,
          address.postalCode,
          address.countryCode,
        )}
      </p>
    </div>
  );
}

export default function ParcelDetailPage() {
  const { id: parcelKey } = useParams<{ id: string }>();
  const router = useRouter();
  const { status: sessionStatus } = useSession();
  const { data: parcel, isLoading, error } = useParcel(parcelKey);
  const cancelParcel = useCancelParcel();
  const downloadParcelLabel = useDownloadParcelLabel();
  const [isCancelDialogOpen, setIsCancelDialogOpen] = useState(false);

  useParcelRealtimeUpdates(parcel);

  useEffect(() => {
    if (
      !parcel ||
      !isGuidString(parcelKey) ||
      parcel.trackingNumber === parcelKey
    ) {
      return;
    }

    router.replace(getParcelDetailPath(parcel.trackingNumber));
  }, [parcel?.trackingNumber, parcelKey, router]);

  async function handleDownload(format: LabelDownloadFormat) {
    if (!parcel) {
      return;
    }

    try {
      await downloadParcelLabel.mutateAsync({
        parcelId: parcel.id,
        format,
      });
    } catch (downloadError) {
      appToast.errorFromUnknown(downloadError);
    }
  }

  if (sessionStatus === "loading" || isLoading) {
    return <DetailPageSkeleton variant="neutral" />;
  }

  if (error) {
    return (
      <DetailShell variant="neutral">
        <DetailContainer className={DETAIL_PAGE_CONTENT_PADDING}>
          <QueryErrorAlert
            title="Could not load parcel"
            message={getErrorMessage(error)}
          />
        </DetailContainer>
      </DetailShell>
    );
  }

  if (!parcel) {
    return (
      <DetailShell variant="neutral">
        <DetailContainer className={DETAIL_PAGE_CONTENT_PADDING}>
          <DetailBreadcrumb
            items={[{ label: "Parcels", href: "/parcels" }, { label: "Not found" }]}
          />
          <DetailEmptyState
            title="Parcel not found"
            message="This parcel may have been removed or the link is incorrect."
          />
        </DetailContainer>
      </DetailShell>
    );
  }

  const recipientName =
    parcel.recipientAddress.contactName ??
    parcel.recipientAddress.companyName ??
    "Recipient";
  const changeHistory = [...parcel.changeHistory].sort(
    (left, right) =>
      new Date(right.changedAt).getTime() - new Date(left.changedAt).getTime(),
  );
  const createdDate = new Date(parcel.createdAt);

  return (
    <DetailShell variant="neutral">
      <DetailContainer className={DETAIL_PAGE_CONTENT_PADDING}>
        <DetailBreadcrumb
          items={[
            { label: "Parcels", href: "/parcels" },
            { label: parcel.trackingNumber },
          ]}
        />

        <DetailHero
          title={parcel.trackingNumber}
          eyebrow="Parcel detail"
          icon={<Package strokeWidth={1.75} />}
          subtitle={
            <>
              {recipientName}
              {" | "}
              {parcel.parcelType ?? "Parcel"}
              {" | "}
              Created {createdDate.toLocaleDateString()}
            </>
          }
          badge={
            <span className={parcelStatusBadgeClass(parcel.status)}>
              {formatParcelStatus(parcel.status)}
            </span>
          }
          actions={
            <div className="flex w-full flex-col gap-3 sm:ml-auto sm:w-auto sm:items-end">
              <div className="flex w-full flex-wrap items-center justify-end gap-2">
                {parcel.canEdit ? (
                  <Link
                    href={getParcelEditPath(parcel.trackingNumber)}
                    className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
                  >
                    <Pencil className="mr-2 size-4" aria-hidden />
                    Edit parcel
                  </Link>
                ) : null}
                {parcel.canCancel ? (
                  <button
                    type="button"
                    className={cn(
                      buttonVariants({ variant: "destructive", size: "sm" }),
                    )}
                    onClick={() => setIsCancelDialogOpen(true)}
                  >
                    Cancel parcel
                  </button>
                ) : null}
                <Link
                  href="/parcels"
                  className={cn(buttonVariants({ variant: "ghost", size: "sm" }))}
                >
                  <ArrowLeft className="mr-2 size-4" aria-hidden />
                  Back to parcels
                </Link>
              </div>
              <div className="flex w-full flex-col gap-2 sm:w-auto sm:items-end">
                <button
                  type="button"
                  className={cn(
                    buttonVariants({ variant: "outline", size: "sm" }),
                    "w-full justify-center sm:w-auto",
                  )}
                  onClick={() => void handleDownload("zpl")}
                  disabled={downloadParcelLabel.isPending}
                >
                  <Printer className="mr-2 size-4" aria-hidden />
                  Reprint 4x6 ZPL
                </button>
                <button
                  type="button"
                  className={cn(
                    buttonVariants({ variant: "outline", size: "sm" }),
                    "w-full justify-center sm:w-auto",
                  )}
                  onClick={() => void handleDownload("pdf")}
                  disabled={downloadParcelLabel.isPending}
                >
                  <Printer className="mr-2 size-4" aria-hidden />
                  Reprint A4 PDF
                </button>
              </div>
            </div>
          }
        />

        <DetailMetricStrip
          items={[
            {
              label: "Parcel type",
              value: parcel.parcelType ?? "-",
              icon: <Package className="size-5" aria-hidden />,
            },
            {
              label: "Created date",
              value: createdDate.toLocaleDateString(),
              icon: <CalendarDays className="size-5" aria-hidden />,
            },
            {
              label: "Delivery zone",
              value: parcel.zoneName ?? "-",
              icon: <MapPin className="size-5" aria-hidden />,
            },
            {
              label: "Weight",
              value: `${parcel.weight} ${parcel.weightUnit}`,
              icon: <Package className="size-5" aria-hidden />,
            },
          ]}
        />

        <DetailPanel
          title="Sender"
          description="Shipper name, address, and contact details."
        >
          <DetailFieldGrid>
            <DetailField label="Name">
              {parcel.senderAddress.contactName ?? "-"}
            </DetailField>
            <DetailField label="Company">
              {parcel.senderAddress.companyName ?? "-"}
            </DetailField>
            <DetailField label="Address">
              {formatAddress(parcel.senderAddress)}
            </DetailField>
            <DetailField label="Phone">{parcel.senderAddress.phone ?? "-"}</DetailField>
            <DetailField label="Email">{parcel.senderAddress.email ?? "-"}</DetailField>
          </DetailFieldGrid>
        </DetailPanel>

        <DetailPanel
          title="Recipient"
          description="Delivery contact, address, and zone details."
        >
          <DetailFieldGrid>
            <DetailField label="Name">
              {parcel.recipientAddress.contactName ?? "-"}
            </DetailField>
            <DetailField label="Company">
              {parcel.recipientAddress.companyName ?? "-"}
            </DetailField>
            <DetailField label="Address">
              {formatAddress(parcel.recipientAddress)}
            </DetailField>
            <DetailField label="Phone">
              {parcel.recipientAddress.phone ?? "-"}
            </DetailField>
            <DetailField label="Email">
              {parcel.recipientAddress.email ?? "-"}
            </DetailField>
            <DetailField label="Delivery zone">{parcel.zoneName ?? "-"}</DetailField>
          </DetailFieldGrid>
        </DetailPanel>

        <DetailPanel
          title="Parcel attributes"
          description="Shipment measurements, notes, and routing metadata."
        >
          <DetailFieldGrid>
            <DetailField label="Service type">
              {formatParcelServiceType(parcel.serviceType)}
            </DetailField>
            <DetailField label="Parcel type">{parcel.parcelType ?? "-"}</DetailField>
            <DetailField label="Weight">
              {parcel.weight} {parcel.weightUnit}
            </DetailField>
            <DetailField label="Dimensions">
              {parcel.length} x {parcel.width} x {parcel.height} {parcel.dimensionUnit}
            </DetailField>
            <DetailField label="Notes">{parcel.description ?? "-"}</DetailField>
            <DetailField label="Declared value">
              {parcel.declaredValue} {parcel.currency}
            </DetailField>
            <DetailField label="Est. delivery date">
              {new Date(parcel.estimatedDeliveryDate).toLocaleDateString()}
            </DetailField>
            <DetailField label="Depot">{parcel.depotName ?? "-"}</DetailField>
            <DetailField label="Delivery attempts">
              {parcel.deliveryAttempts.toString()}
            </DetailField>
            <DetailField label="Last modified">
              {parcel.lastModifiedAt
                ? new Date(parcel.lastModifiedAt).toLocaleString()
                : "-"}
            </DetailField>
            <DetailField label="Cancellation reason">
              {parcel.cancellationReason ?? "-"}
            </DetailField>
          </DetailFieldGrid>
        </DetailPanel>

        <ParcelTimeline events={parcel.statusTimeline} />

        <DetailPanel
          title="Related route"
          description="Current delivery assignment for this parcel, when one exists."
        >
          {parcel.routeAssignment ? (
            <DetailFieldGrid>
              <DetailField label="Route">
                <Link
                  href={`/routes/${parcel.routeAssignment.routeId}`}
                  className="inline-flex items-center gap-2 text-primary underline-offset-4 hover:underline"
                >
                  <Route className="size-4" aria-hidden />
                  {parcel.routeAssignment.routeId}
                </Link>
              </DetailField>
              <DetailField label="Route status">
                {formatParcelStatus(parcel.routeAssignment.routeStatus)}
              </DetailField>
              <DetailField label="Driver">
                <span className="inline-flex items-center gap-2">
                  <UserRound className="size-4" aria-hidden />
                  {parcel.routeAssignment.driverName}
                </span>
              </DetailField>
              <DetailField label="Vehicle">
                <span className="inline-flex items-center gap-2">
                  <Truck className="size-4" aria-hidden />
                  {parcel.routeAssignment.vehiclePlate}
                </span>
              </DetailField>
              <DetailField label="Start">
                {new Date(parcel.routeAssignment.startDate).toLocaleString()}
              </DetailField>
              <DetailField label="End">
                {parcel.routeAssignment.endDate
                  ? new Date(parcel.routeAssignment.endDate).toLocaleString()
                  : "-"}
              </DetailField>
            </DetailFieldGrid>
          ) : (
            <p className="text-sm text-muted-foreground">
              This parcel has not been assigned to a route yet.
            </p>
          )}
        </DetailPanel>

        {parcel.proofOfDelivery ? (
          <DetailPanel
            title="Proof of delivery"
            description="Delivery confirmation metadata captured for this parcel."
          >
            <DetailFieldGrid>
              <DetailField label="Received by">
                {parcel.proofOfDelivery.receivedBy ?? "-"}
              </DetailField>
              <DetailField label="Delivery location">
                {parcel.proofOfDelivery.deliveryLocation ?? "-"}
              </DetailField>
              <DetailField label="Delivered at">
                {new Date(parcel.proofOfDelivery.deliveredAt).toLocaleString()}
              </DetailField>
              <DetailField label="Signature image">
                {parcel.proofOfDelivery.hasSignatureImage ? "Available" : "Not captured"}
              </DetailField>
              <DetailField label="Delivery photo">
                {parcel.proofOfDelivery.hasPhoto ? "Available" : "Not captured"}
              </DetailField>
            </DetailFieldGrid>
          </DetailPanel>
        ) : null}

        <DetailPanel
          title="Change history"
          description="Field-level edits and cancellation activity, newest changes first."
        >
          {changeHistory.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No changes have been recorded for this parcel yet.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead>
                  <tr className="border-b border-border/60 text-left text-xs uppercase tracking-[0.14em] text-muted-foreground">
                    <th className="px-3 py-3 font-semibold">When</th>
                    <th className="px-3 py-3 font-semibold">Who</th>
                    <th className="px-3 py-3 font-semibold">Action</th>
                    <th className="px-3 py-3 font-semibold">Field</th>
                    <th className="px-3 py-3 font-semibold">Before</th>
                    <th className="px-3 py-3 font-semibold">After</th>
                  </tr>
                </thead>
                <tbody>
                  {changeHistory.map((entry, index) => (
                    <tr
                      key={`${entry.changedAt}-${entry.fieldName}-${index}`}
                      className="border-b border-border/40 last:border-b-0"
                    >
                      <td className="px-3 py-3 align-top tabular-nums">
                        {new Date(entry.changedAt).toLocaleString()}
                      </td>
                      <td className="px-3 py-3 align-top">
                        {entry.changedBy ?? "System"}
                      </td>
                      <td className="px-3 py-3 align-top">{entry.action}</td>
                      <td className="px-3 py-3 align-top">{entry.fieldName}</td>
                      <td className="px-3 py-3 align-top text-muted-foreground">
                        {formatHistoryValue(entry.beforeValue)}
                      </td>
                      <td className="px-3 py-3 align-top">
                        {formatHistoryValue(entry.afterValue)}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </DetailPanel>

        <ParcelStatusTransition parcel={parcel} />
      </DetailContainer>

      <CancelParcelDialog
        open={isCancelDialogOpen}
        onOpenChange={setIsCancelDialogOpen}
        trackingNumber={parcel.trackingNumber}
        onConfirm={async (reason) => {
          await cancelParcel.mutateAsync({ id: parcel.id, reason });
          setIsCancelDialogOpen(false);
        }}
        isPending={cancelParcel.isPending}
      />
    </DetailShell>
  );
}
