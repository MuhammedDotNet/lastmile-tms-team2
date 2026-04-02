"use client";

import { useParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Pencil } from "lucide-react";
import { useSession } from "next-auth/react";

import {
  DetailBreadcrumb,
  DetailEmptyState,
  DetailFormPageShell,
  DetailPageSkeleton,
  FORM_PAGE_FORM_COLUMN_CLASS,
} from "@/components/detail";
import { ListPageHeader } from "@/components/list";
import { buttonVariants } from "@/components/ui/button";
import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import { cn } from "@/lib/utils";
import { getErrorMessage } from "@/lib/network/error-message";
import { useDriver } from "@/queries/drivers";
import { DriverForm } from "@/components/drivers/driver-form";

export default function DriverEditPage() {
  const { id } = useParams<{ id: string }>();
  const { status: sessionStatus } = useSession();
  const { data: driver, isLoading, error } = useDriver(id);

  if (sessionStatus === "loading" || isLoading)
    return <DetailPageSkeleton variant="driver" />;
  if (error)
    return (
      <DetailFormPageShell variant="driver">
        <DetailBreadcrumb
          className="form-page-breadcrumb-animate"
          variant="driver"
          items={[
            { label: "Drivers", href: "/drivers" },
            { label: "Edit" },
          ]}
        />
        <div className={FORM_PAGE_FORM_COLUMN_CLASS}>
          <QueryErrorAlert
            title="Could not load driver"
            message={getErrorMessage(error)}
          />
        </div>
      </DetailFormPageShell>
    );
  if (!driver)
    return (
      <DetailFormPageShell variant="driver">
        <DetailBreadcrumb
          className="form-page-breadcrumb-animate"
          variant="driver"
          items={[
            { label: "Drivers", href: "/drivers" },
            { label: "Edit" },
          ]}
        />
        <div className={FORM_PAGE_FORM_COLUMN_CLASS}>
          <DetailEmptyState
            title="Driver not found"
            message="This driver may have been removed or the link is incorrect."
          />
        </div>
      </DetailFormPageShell>
    );

  return (
    <DetailFormPageShell variant="driver">
      <DetailBreadcrumb
        className="form-page-breadcrumb-animate"
        variant="driver"
        items={[
          { label: "Drivers", href: "/drivers" },
          { label: driver.displayName, href: `/drivers/${id}` },
          { label: "Edit" },
        ]}
      />

      <ListPageHeader
        eyebrow="Drivers"
        title="Edit driver"
        description={`Changes apply to ${driver.displayName}.`}
        icon={<Pencil strokeWidth={1.75} />}
        action={
          <Link
            href={`/drivers/${id}`}
            className={cn(buttonVariants({ variant: "outline", size: "sm" }))}
          >
            <ArrowLeft className="mr-2 size-4" aria-hidden />
            Back to driver
          </Link>
        }
      />

      <div className={cn(FORM_PAGE_FORM_COLUMN_CLASS, "form-page-body-animate")}>
        <DriverForm key={id} mode="edit" driver={driver} />
      </div>
    </DetailFormPageShell>
  );
}
