"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import { ArrowLeft, Route as RouteIcon, ScanLine, Truck, User } from "lucide-react";
import { useSession } from "next-auth/react";

import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import { Button, buttonVariants } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  ROUTE_STATUS_LABELS,
  STAGING_AREA_LABELS,
  routeStatusBadgeClass,
} from "@/lib/labels/routes";
import { getErrorMessage } from "@/lib/network/error-message";
import { cn } from "@/lib/utils";
import { getParcelDetailPath } from "@/lib/parcels/paths";
import {
  useRouteStagingBoard,
  useStageParcelForRoute,
  useStagingRoutes,
} from "@/queries/parcels";
import type {
  RouteStagingBoard,
  StageParcelForRouteResult,
} from "@/types/parcels";

function CountCard({
  label,
  value,
}: {
  label: string;
  value: number;
}) {
  return (
    <div className="rounded-2xl border border-border/60 bg-card/80 p-4 shadow-sm">
      <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
        {label}
      </p>
      <p className="mt-2 text-3xl font-semibold text-foreground">{value}</p>
    </div>
  );
}

function ScanMessage({
  result,
}: {
  result: StageParcelForRouteResult | null;
}) {
  if (!result) {
    return null;
  }

  const toneClass =
    result.outcome === "STAGED" || result.outcome === "ALREADY_STAGED"
      ? "border-emerald-300/60 bg-emerald-50/70 text-emerald-950"
      : "border-amber-300/60 bg-amber-50/80 text-amber-950";

  return (
    <div className={cn("rounded-2xl border px-4 py-3 text-sm shadow-sm", toneClass)}>
      <p className="font-medium">{result.message}</p>
      {result.trackingNumber ? (
        <p className="mt-1 font-mono text-xs uppercase tracking-[0.14em]">
          {result.trackingNumber}
        </p>
      ) : null}
      {result.conflictingStagingArea ? (
        <p className="mt-1 text-xs">
          Assigned staging area:{" "}
          <span className="font-semibold">
            {STAGING_AREA_LABELS[result.conflictingStagingArea]}
          </span>
        </p>
      ) : null}
    </div>
  );
}

function ParcelStageRow({
  parcel,
}: {
  parcel: RouteStagingBoard["expectedParcels"][number];
}) {
  return (
    <div className="flex items-center justify-between gap-4 rounded-2xl border border-border/60 bg-background/75 px-4 py-3">
      <div className="min-w-0">
        <Link
          href={getParcelDetailPath(parcel.trackingNumber)}
          className="font-mono text-sm font-medium text-primary underline-offset-4 hover:underline"
        >
          {parcel.trackingNumber}
        </Link>
        <p className="mt-1 text-xs text-muted-foreground">{parcel.status}</p>
      </div>
      <span
        className={cn(
          "shrink-0 rounded-full px-2.5 py-1 text-xs font-semibold",
          parcel.isStaged
            ? "bg-emerald-100 text-emerald-900 dark:bg-emerald-950/50 dark:text-emerald-200"
            : "bg-muted text-muted-foreground",
        )}
      >
        {parcel.isStaged ? "Staged" : "Pending"}
      </span>
    </div>
  );
}

export function RouteStagingPage() {
  const { status: sessionStatus } = useSession();
  const { data: stagingRoutes = [], isLoading, error } = useStagingRoutes();

  const [selectedRouteId, setSelectedRouteId] = useState<string>("");
  const effectiveSelectedRouteId = useMemo(() => {
    if (
      selectedRouteId &&
      stagingRoutes.some((route) => route.id === selectedRouteId)
    ) {
      return selectedRouteId;
    }

    return stagingRoutes[0]?.id ?? "";
  }, [selectedRouteId, stagingRoutes]);

  const selectedRoute = useMemo(
    () =>
      stagingRoutes.find((route) => route.id === effectiveSelectedRouteId) ??
      stagingRoutes[0] ??
      null,
    [effectiveSelectedRouteId, stagingRoutes],
  );

  if (sessionStatus === "loading" || isLoading) {
    return (
      <div className="rounded-2xl border border-border/60 bg-card/80 p-6 shadow-sm">
        Loading route staging...
      </div>
    );
  }

  if (error) {
    return (
      <QueryErrorAlert
        title="Could not load staging routes"
        message={getErrorMessage(error)}
      />
    );
  }

  if (stagingRoutes.length === 0) {
    return (
      <div className="space-y-6">
        <div className="rounded-3xl border border-border/60 bg-card/85 p-6 shadow-sm">
          <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
            <div className="space-y-2">
              <div className="inline-flex items-center gap-2 rounded-full border border-border/60 bg-background/60 px-3 py-1 text-xs uppercase tracking-[0.18em] text-muted-foreground">
                <RouteIcon className="size-3.5" aria-hidden />
                Warehouse staging
              </div>
              <h1 className="text-3xl font-semibold tracking-tight text-foreground">
                Route staging
              </h1>
              <p className="max-w-3xl text-sm text-muted-foreground">
                Select an active route and stage sorted parcels into its assigned
                lane before vehicle load-out.
              </p>
            </div>
            <Link
              href="/parcels"
              className={cn(buttonVariants({ variant: "ghost" }), "self-start")}
            >
              <ArrowLeft className="size-4" aria-hidden />
              Back to parcels
            </Link>
          </div>
        </div>

        <div className="rounded-2xl border border-dashed border-border p-10 text-center">
          <p className="font-medium">No active routes are waiting for staging</p>
          <p className="mt-2 text-sm text-muted-foreground">
            Planned and in-progress depot routes will appear here when parcels
            are assigned for load-out.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="rounded-3xl border border-border/60 bg-card/85 p-6 shadow-sm">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-2">
            <div className="inline-flex items-center gap-2 rounded-full border border-border/60 bg-background/60 px-3 py-1 text-xs uppercase tracking-[0.18em] text-muted-foreground">
              <RouteIcon className="size-3.5" aria-hidden />
              Warehouse staging
            </div>
            <h1 className="text-3xl font-semibold tracking-tight text-foreground">
              Route staging
            </h1>
            <p className="max-w-3xl text-sm text-muted-foreground">
              Confirm that each parcel reaches the correct lane prior to vehicle
              load-out.
            </p>
          </div>
          <Link
            href="/parcels"
            className={cn(buttonVariants({ variant: "ghost" }), "self-start")}
          >
            <ArrowLeft className="size-4" aria-hidden />
            Back to parcels
          </Link>
        </div>
      </div>

      <div className="rounded-2xl border border-border/60 bg-card/80 p-4 shadow-sm">
        <div className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_auto] lg:items-end">
          <div className="space-y-2">
            <Label htmlFor="route-select">Delivery route</Label>
            <select
              id="route-select"
              className="h-10 w-full rounded-xl border border-input bg-background px-3 text-sm shadow-sm"
              value={effectiveSelectedRouteId}
              onChange={(event) => setSelectedRouteId(event.target.value)}
            >
              {stagingRoutes.map((route) => (
                <option key={route.id} value={route.id}>
                  {route.vehiclePlate} | {route.driverName}
                </option>
              ))}
            </select>
          </div>

          {selectedRoute ? (
            <div className="rounded-2xl border border-border/60 bg-background/70 px-4 py-3 text-sm shadow-sm">
              <div className="flex flex-wrap items-center gap-3">
                <span className="inline-flex items-center gap-2 font-medium text-foreground">
                  <Truck className="size-4" aria-hidden />
                  {selectedRoute.vehiclePlate}
                </span>
                <span className="inline-flex items-center gap-2 text-muted-foreground">
                  <User className="size-4" aria-hidden />
                  {selectedRoute.driverName}
                </span>
              </div>
            </div>
          ) : null}
        </div>
      </div>

      {selectedRoute ? (
        <RouteStagingBoardPanel
          key={selectedRoute.id}
          routeId={selectedRoute.id}
        />
      ) : null}
    </div>
  );
}

export default RouteStagingPage;

function RouteStagingBoardPanel({
  routeId,
}: {
  routeId: string;
}) {
  const stageParcelForRoute = useStageParcelForRoute();
  const [barcode, setBarcode] = useState("");
  const [boardSnapshot, setBoardSnapshot] = useState<RouteStagingBoard | null>(null);
  const [lastScanResult, setLastScanResult] =
    useState<StageParcelForRouteResult | null>(null);

  const { data: queriedBoard, isLoading, error } = useRouteStagingBoard(routeId);
  const board = boardSnapshot?.id === routeId ? boardSnapshot : queriedBoard ?? null;

  async function handleScanSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!barcode.trim()) {
      return;
    }

    const result = await stageParcelForRoute.mutateAsync({
      routeId,
      barcode: barcode.trim(),
    });

    setBoardSnapshot(result.board);
    setLastScanResult(result);
    setBarcode("");
  }

  if (error) {
    return (
      <QueryErrorAlert
        title="Could not load the staging board"
        message={getErrorMessage(error)}
      />
    );
  }

  if (isLoading && !board) {
    return (
      <div className="rounded-2xl border border-border/60 bg-card/80 p-6 shadow-sm">
        Loading staging board...
      </div>
    );
  }

  if (!board) {
    return null;
  }

  return (
    <>
      <div className="rounded-2xl border border-border/60 bg-card/80 p-5 shadow-sm">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-start lg:justify-between">
          <div className="space-y-2">
            <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">
              Assigned staging area
            </p>
            <div className="flex flex-wrap items-center gap-3">
              <h2 className="text-2xl font-semibold tracking-tight text-foreground">
                {STAGING_AREA_LABELS[board.stagingArea]}
              </h2>
              <span className={routeStatusBadgeClass(board.status)}>
                {ROUTE_STATUS_LABELS[board.status]}
              </span>
            </div>
          </div>
          <p className="text-sm text-muted-foreground">
            Departure: {new Date(board.startDate).toLocaleString()}
          </p>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <CountCard label="Expected parcels" value={board.expectedParcelCount} />
        <CountCard label="Staged parcels" value={board.stagedParcelCount} />
        <CountCard label="Remaining parcels" value={board.remainingParcelCount} />
      </div>

      <div className="rounded-2xl border border-border/60 bg-card/80 p-4 shadow-sm">
        <form className="space-y-3" onSubmit={(event) => void handleScanSubmit(event)}>
          <div className="space-y-2">
            <Label htmlFor="scan-barcode">Scan barcode</Label>
            <Input
              id="scan-barcode"
              autoFocus
              autoComplete="off"
              value={barcode}
              onChange={(event) => setBarcode(event.target.value)}
              placeholder="Scan or type the parcel barcode"
            />
          </div>
          <div className="flex flex-wrap items-center gap-3">
            <Button
              type="submit"
              disabled={stageParcelForRoute.isPending || !barcode.trim()}
            >
              <ScanLine className="size-4" aria-hidden />
              {stageParcelForRoute.isPending ? "Staging..." : "Stage parcel"}
            </Button>
          </div>
        </form>
      </div>

      <ScanMessage result={lastScanResult} />

      <section className="rounded-2xl border border-border/60 bg-card/80 p-4 shadow-sm">
        <div className="flex items-center justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold text-foreground">
              Assigned parcel list
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              Parcels assigned to this route and awaiting staging confirmation.
            </p>
          </div>
        </div>
        <div className="mt-4 space-y-3">
          {board.expectedParcels.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No expected parcels are assigned to this route yet.
            </p>
          ) : (
            board.expectedParcels.map((parcel) => (
              <ParcelStageRow key={parcel.parcelId} parcel={parcel} />
            ))
          )}
        </div>
      </section>
    </>
  );
}
