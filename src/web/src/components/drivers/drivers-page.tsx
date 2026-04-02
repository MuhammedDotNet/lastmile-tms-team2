"use client";

import { useMemo, useState } from "react";
import Image from "next/image";
import Link from "next/link";
import { Plus, UserCircle } from "lucide-react";
import { useSession } from "next-auth/react";

import {
  ListDataTable,
  ListPageHeader,
  ListPageLoading,
  ListPagePagination,
  ListPageStatsStrip,
  listDataTableBodyRowClass,
  listDataTableHeadRowClass,
  listDataTableTdClass,
  listDataTableThClass,
  listDataTableThRightClass,
} from "@/components/list";
import { DeleteDriverDialog } from "@/components/drivers/delete-driver-dialog";
import { DriverRowActions } from "@/components/drivers/driver-row-actions";
import { buttonVariants } from "@/components/ui/button";
import { getErrorMessage } from "@/lib/network/error-message";
import {
  DRIVER_STATUS_LABELS,
  driverStatusBadgeClass,
} from "@/lib/labels/drivers";
import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import { useDrivers, useDeleteDriver } from "@/queries/drivers";
import { DriverStatus } from "@/types/drivers";
import { DepotFilterDropdown } from "@/components/vehicles/depot-filter-dropdown";
import { DriverStatusFilter } from "@/components/drivers/status-filter-dropdown";
import { OverflowTooltipCell } from "@/components/list/overflow-tooltip-cell";
import { cn } from "@/lib/utils";
import { useDepots } from "@/queries/depots";
import { absoluteApiAssetUrl } from "@/lib/network/api";

const PAGE_SIZE = 20;

function DriverListAvatar({
  photoUrl,
  name,
}: {
  photoUrl: string | null | undefined;
  name: string;
}) {
  const src = absoluteApiAssetUrl(photoUrl);
  const [imgError, setImgError] = useState(false);

  if (!src || imgError) {
    return (
      <div
        className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full border border-border bg-muted text-muted-foreground"
        aria-hidden
      >
        <UserCircle className="h-5 w-5" strokeWidth={1.5} />
      </div>
    );
  }

  return (
    <Image
      src={src}
      alt=""
      width={36}
      height={36}
      sizes="36px"
      unoptimized
      className="h-9 w-9 shrink-0 rounded-full border border-border object-cover"
      onError={() => setImgError(true)}
      title={name}
    />
  );
}

export default function DriversPage() {
  const { status: sessionStatus } = useSession();
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<DriverStatus | undefined>();
  const [depotFilter, setDepotFilter] = useState<string | undefined>();
  const [deleteTarget, setDeleteTarget] = useState<{
    id: string;
    displayName: string;
  } | null>(null);

  const { data: depotsData } = useDepots();
  const { data = [], isLoading, error } = useDrivers({
    status: statusFilter,
    depotId: depotFilter,
  });

  const deleteDriver = useDeleteDriver();

  const total = data.length;
  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));
  const from = total === 0 ? 0 : (page - 1) * PAGE_SIZE + 1;
  const to = total === 0 ? 0 : Math.min(page * PAGE_SIZE, total);
  const pagedItems = useMemo(
    () => data.slice((page - 1) * PAGE_SIZE, page * PAGE_SIZE),
    [data, page],
  );

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    try {
      await deleteDriver.mutateAsync({
        id: deleteTarget.id,
        displayName: deleteTarget.displayName,
      });
      setDeleteTarget(null);
    } catch {
      /* error toast from global MutationCache */
    }
  };

  if (sessionStatus === "loading" || isLoading) return <ListPageLoading />;
  if (error)
    return (
      <QueryErrorAlert
        title="Could not load drivers"
        message={getErrorMessage(error)}
      />
    );

  return (
    <>
      <DeleteDriverDialog
        open={deleteTarget !== null}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        displayName={deleteTarget?.displayName ?? ""}
        onConfirm={confirmDelete}
        isPending={deleteDriver.isPending}
      />

      <ListPageHeader
        title="Drivers"
        description="Fleet drivers: profiles, licenses, zone assignments, and availability."
        action={
          <Link
            href="/drivers/new"
            className={cn(
              buttonVariants({ size: "default" }),
              "shrink-0 gap-2 self-start sm:self-auto",
            )}
          >
            <Plus className="size-4" aria-hidden />
            Add driver
          </Link>
        }
      />

      <ListPageStatsStrip
        totalLabel="Total drivers"
        totalCount={total}
        rangeEntityLabel="drivers"
        from={from}
        to={to}
        page={page}
        totalPages={totalPages}
        pageSize={PAGE_SIZE}
        filterCardLabel="Status filter"
        filterCardHint="Use the filter below to narrow the list"
        activeFilterDisplay={
          statusFilter !== undefined
            ? DRIVER_STATUS_LABELS[statusFilter]
            : "All statuses"
        }
      />

      <div className="list-page-filters-animate mb-6 flex gap-3">
        <DriverStatusFilter
          value={statusFilter}
          onChange={(value) => {
            setStatusFilter(value);
            setPage(1);
          }}
        />
        <DepotFilterDropdown
          value={depotFilter}
          onChange={(value) => {
            setDepotFilter(value);
            setPage(1);
          }}
          depots={depotsData}
        />
      </div>

      <ListDataTable minWidthClassName="min-w-[900px]">
        <thead>
          <tr className={listDataTableHeadRowClass}>
            <th className={listDataTableThClass}>Name</th>
            <th className={listDataTableThClass}>License</th>
            <th className={listDataTableThClass}>Status</th>
            <th className={cn(listDataTableThClass, "max-w-[160px]")}>
              Depot
            </th>
            <th className={cn(listDataTableThClass, "max-w-[160px]")}>
              Zone
            </th>
            <th className={cn(listDataTableThClass, "max-w-[200px]")}>
              Contact
            </th>
            <th className={listDataTableThRightClass}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {pagedItems.map((driver) => (
            <tr key={driver.id} className={listDataTableBodyRowClass}>
              <td className={cn(listDataTableTdClass, "max-w-[260px]")}>
                <div className="flex min-w-0 items-center gap-3">
                  <DriverListAvatar
                    photoUrl={driver.photoUrl}
                    name={driver.displayName}
                  />
                  <Link
                    href={`/drivers/${driver.id}`}
                    className="min-w-0 truncate font-medium hover:underline"
                  >
                    {driver.displayName}
                  </Link>
                </div>
              </td>
              <td className={cn(listDataTableTdClass, "max-w-[150px]")}>
                <OverflowTooltipCell
                  fullText={`${driver.licenseNumber}${
                    driver.licenseExpiryDate
                      ? ` (exp: ${new Date(driver.licenseExpiryDate).toLocaleDateString()})`
                      : ""
                  }`}
                />
              </td>
              <td className={cn(listDataTableTdClass, "max-w-[160px] align-middle")}>
                <OverflowTooltipCell
                  shrinkToContent
                  fullText={DRIVER_STATUS_LABELS[driver.status]}
                  className={driverStatusBadgeClass(driver.status)}
                />
              </td>
              <td className={cn(listDataTableTdClass, "max-w-[160px] text-muted-foreground")}>
                <OverflowTooltipCell fullText={driver.depotName || ""} />
              </td>
              <td className={cn(listDataTableTdClass, "max-w-[160px] text-muted-foreground")}>
                <OverflowTooltipCell fullText={driver.zoneName || ""} />
              </td>
              <td className={cn(listDataTableTdClass, "max-w-[200px] text-muted-foreground")}>
                <OverflowTooltipCell
                  fullText={
                    driver.phone
                      ? `${driver.phone}${driver.email ? ` / ${driver.email}` : ""}`
                      : driver.email || ""
                  }
                />
              </td>
              <td className={cn(listDataTableTdClass, "min-w-32 text-right align-middle")}>
                <DriverRowActions
                  driverId={driver.id}
                  displayName={driver.displayName}
                  onDeleteAction={() =>
                    setDeleteTarget({
                      id: driver.id,
                      displayName: driver.displayName,
                    })
                  }
                  deleteDisabled={deleteDriver.isPending}
                />
              </td>
            </tr>
          ))}
        </tbody>
      </ListDataTable>

      {totalPages > 1 && (
        <ListPagePagination
          page={page}
          totalPages={totalPages}
          setPage={setPage}
        />
      )}
    </>
  );
}
