"use client";

import { useEffect } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, PencilLine } from "lucide-react";
import { useSession } from "next-auth/react";

import {
  DetailBreadcrumb,
  DetailEmptyState,
  DetailFormPageShell,
  DetailPageSkeleton,
  FORM_PAGE_FORM_COLUMN_CLASS,
} from "@/components/detail";
import { ListPageHeader } from "@/components/list";
import { ParcelEditorForm } from "@/components/parcels/parcel-editor-form";
import { buttonVariants } from "@/components/ui/button";
import { parcelDetailToFormData } from "@/lib/parcels/forms";
import {
  getParcelDetailPath,
  getParcelEditPath,
} from "@/lib/parcels/paths";
import { cn } from "@/lib/utils";
import { isGuidString } from "@/lib/validation/guid-string";
import { useParcel, useUpdateParcel } from "@/queries/parcels";

export default function ParcelEditPage() {
  const { id: parcelKey } = useParams<{ id: string }>();
  const router = useRouter();
  const { status: sessionStatus } = useSession();
  const { data: parcel, isLoading } = useParcel(parcelKey);
  const updateParcel = useUpdateParcel();

  useEffect(() => {
    if (
      !parcel ||
      !isGuidString(parcelKey) ||
      parcel.trackingNumber === parcelKey
    ) {
      return;
    }

    router.replace(getParcelEditPath(parcel.trackingNumber));
  }, [parcel?.trackingNumber, parcelKey, router]);

  if (sessionStatus === "loading" || isLoading) {
    return <DetailPageSkeleton variant="neutral" />;
  }

  if (!parcel) {
    return (
      <DetailFormPageShell variant="neutral">
        <DetailBreadcrumb
          items={[
            { label: "Parcels", href: "/parcels" },
            { label: "Edit" },
          ]}
        />
        <div className={FORM_PAGE_FORM_COLUMN_CLASS}>
          <DetailEmptyState
            title="Parcel not found"
            message="This parcel may have been removed or the link is incorrect."
          />
        </div>
      </DetailFormPageShell>
    );
  }

  const detailPath = getParcelDetailPath(parcel.trackingNumber);

  if (!parcel.canEdit) {
    return (
      <DetailFormPageShell variant="neutral">
        <DetailBreadcrumb
          items={[
            { label: "Parcels", href: "/parcels" },
            { label: parcel.trackingNumber, href: detailPath },
            { label: "Edit" },
          ]}
        />
        <div className={FORM_PAGE_FORM_COLUMN_CLASS}>
          <DetailEmptyState
            title="Parcel can no longer be edited"
            message="Only parcels in Registered, Received at Depot, Sorted, or Staged can be changed."
          />
        </div>
      </DetailFormPageShell>
    );
  }

  return (
    <DetailFormPageShell variant="neutral">
      <DetailBreadcrumb
        items={[
          { label: "Parcels", href: "/parcels" },
          { label: parcel.trackingNumber, href: detailPath },
          { label: "Edit" },
        ]}
      />

      <ListPageHeader
        eyebrow="Warehouse"
        title="Edit parcel"
        description={`Update recipient or shipment details for ${parcel.trackingNumber}.`}
        icon={<PencilLine strokeWidth={1.75} />}
        action={
          <Link
            href={detailPath}
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            <ArrowLeft className="mr-2 size-4" aria-hidden />
            Back to parcel
          </Link>
        }
      />

      <div className={cn(FORM_PAGE_FORM_COLUMN_CLASS, "space-y-6")}>
        <ParcelEditorForm
          key={parcel.id}
          initialData={parcelDetailToFormData(parcel)}
          onSubmit={async (form) => {
            await updateParcel.mutateAsync({ id: parcel.id, data: form });
            router.push(detailPath);
          }}
          onCancel={() => router.push(detailPath)}
          submitLabel="Save changes"
          pendingLabel="Saving..."
          error={updateParcel.isError ? updateParcel.error : undefined}
          isPending={updateParcel.isPending}
        />
      </div>
    </DetailFormPageShell>
  );
}
