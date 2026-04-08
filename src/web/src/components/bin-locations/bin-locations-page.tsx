"use client";

import { useState } from "react";
import { Boxes, Plus } from "lucide-react";
import { useSession } from "next-auth/react";
import {
  ListPageHeader,
  ListPageLoading,
  ListPageStatsStrip,
} from "@/components/list";
import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import { NameForm } from "@/components/bin-locations/bin-location-forms";
import { BinLocationDeleteDialog } from "@/components/bin-locations/bin-location-delete-dialog";
import { BinLocationsTree } from "@/components/bin-locations/bin-locations-tree";
import type {
  CreateState,
  DeleteState,
  EditState,
} from "@/components/bin-locations/bin-locations-state";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { getErrorMessage } from "@/lib/network/error-message";
import {
  useCreateBinLocation,
  useCreateStorageAisle,
  useCreateStorageZone,
  useDeleteBinLocation,
  useDeleteStorageAisle,
  useDeleteStorageZone,
  useDepotStorageLayout,
  useUpdateBinLocation,
  useUpdateStorageAisle,
  useUpdateStorageZone,
} from "@/queries/bin-locations";
import { useDepots } from "@/queries/depots";
import type {
  BinLocation,
  StorageAisle,
  StorageZone,
} from "@/types/bin-locations";

function findStorageAisle(storageZones: StorageZone[], aisleId: string): StorageAisle | null {
  for (const zone of storageZones) {
    const aisle = zone.storageAisles.find((candidate) => candidate.id === aisleId);
    if (aisle) {
      return aisle;
    }
  }

  return null;
}

function findBinLocation(storageZones: StorageZone[], binId: string): BinLocation | null {
  for (const zone of storageZones) {
    for (const aisle of zone.storageAisles) {
      const bin = aisle.binLocations.find((candidate) => candidate.id === binId);
      if (bin) {
        return bin;
      }
    }
  }

  return null;
}

export default function BinLocationsPage() {
  const { status } = useSession();
  const { data: depots, isLoading: depotsLoading, error: depotsError } = useDepots();
  const [selectedDepotId, setSelectedDepotId] = useState("");
  const [collapsedZoneIds, setCollapsedZoneIds] = useState<Set<string>>(new Set());
  const [createState, setCreateState] = useState<CreateState>(null);
  const [editState, setEditState] = useState<EditState>(null);
  const [deleteState, setDeleteState] = useState<DeleteState>(null);
  const [draftName, setDraftName] = useState("");
  const [draftIsActive, setDraftIsActive] = useState(true);
  const [draftDeliveryZoneId, setDraftDeliveryZoneId] = useState("");
  const [submitError, setSubmitError] = useState<string | undefined>();

  const activeDepotId = depots?.some((depot) => depot.id === selectedDepotId)
    ? selectedDepotId
    : depots?.[0]?.id ?? "";

  const layoutQuery = useDepotStorageLayout(activeDepotId);
  const createStorageZone = useCreateStorageZone();
  const updateStorageZone = useUpdateStorageZone();
  const deleteStorageZone = useDeleteStorageZone();
  const createStorageAisle = useCreateStorageAisle();
  const updateStorageAisle = useUpdateStorageAisle();
  const deleteStorageAisle = useDeleteStorageAisle();
  const createBinLocation = useCreateBinLocation();
  const updateBinLocation = useUpdateBinLocation();
  const deleteBinLocation = useDeleteBinLocation();

  const layout = layoutQuery.data;
  const storageZones = layout?.storageZones ?? [];
  const deliveryZoneOptions = layout?.availableDeliveryZones ?? [];
  const queryError = depotsError ?? layoutQuery.error;
  const isLoading =
    status === "loading"
    || depotsLoading
    || (Boolean(activeDepotId) && layoutQuery.isLoading);

  let totalAisles = 0;
  let totalBins = 0;
  for (const zone of storageZones) {
    totalAisles += zone.storageAisles.length;
    for (const aisle of zone.storageAisles) {
      totalBins += aisle.binLocations.length;
    }
  }

  const currentEditingBinId = editState?.kind === "bin" ? editState.bin.id : null;
  const disabledDeliveryZoneIds = new Set<string>();
  if (draftIsActive) {
    for (const zone of storageZones) {
      for (const aisle of zone.storageAisles) {
        for (const bin of aisle.binLocations) {
          if (
            bin.isActive
            && bin.deliveryZoneId
            && bin.id !== currentEditingBinId
          ) {
            disabledDeliveryZoneIds.add(bin.deliveryZoneId);
          }
        }
      }
    }
  }

  function resetDrafts() {
    setCreateState(null);
    setEditState(null);
    setDraftName("");
    setDraftIsActive(true);
    setDraftDeliveryZoneId("");
    setSubmitError(undefined);
  }

  function toggleZone(zoneId: string) {
    setCollapsedZoneIds((current) => {
      const next = new Set(current);
      if (next.has(zoneId)) {
        next.delete(zoneId);
      } else {
        next.add(zoneId);
      }
      return next;
    });
  }

  function startCreateZone() {
    setCreateState({ kind: "zone" });
    setEditState(null);
    setDraftName("");
    setDraftIsActive(true);
    setDraftDeliveryZoneId("");
    setSubmitError(undefined);
  }

  function startCreateAisle(storageZoneId: string) {
    setCreateState({ kind: "aisle", storageZoneId });
    setEditState(null);
    setDraftName("");
    setDraftIsActive(true);
    setDraftDeliveryZoneId("");
    setSubmitError(undefined);
    setCollapsedZoneIds((current) => {
      const next = new Set(current);
      next.delete(storageZoneId);
      return next;
    });
  }

  function startCreateBin(storageAisleId: string) {
    setCreateState({ kind: "bin", storageAisleId });
    setEditState(null);
    setDraftName("");
    setDraftIsActive(true);
    setDraftDeliveryZoneId("");
    setSubmitError(undefined);
  }

  function startEditZone(zone: StorageZone) {
    setEditState({ kind: "zone", zone });
    setCreateState(null);
    setDraftName(zone.name);
    setDraftIsActive(true);
    setDraftDeliveryZoneId("");
    setSubmitError(undefined);
  }

  function startEditAisle(aisleId: string) {
    const aisle = findStorageAisle(storageZones, aisleId);
    if (!aisle) {
      return;
    }

    setEditState({ kind: "aisle", aisle });
    setCreateState(null);
    setDraftName(aisle.name);
    setDraftIsActive(true);
    setDraftDeliveryZoneId("");
    setSubmitError(undefined);
  }

  function startEditBin(binId: string) {
    const bin = findBinLocation(storageZones, binId);
    if (!bin) {
      return;
    }

    setEditState({ kind: "bin", bin });
    setCreateState(null);
    setDraftName(bin.name);
    setDraftIsActive(bin.isActive);
    setDraftDeliveryZoneId(bin.deliveryZoneId ?? "");
    setSubmitError(undefined);
  }

  async function handleCreateSubmit(event: React.FormEvent) {
    event.preventDefault();

    const name = draftName.trim();
    if (!name) {
      setSubmitError("Name is required.");
      return;
    }

    try {
      if (createState?.kind === "zone" && activeDepotId) {
        await createStorageZone.mutateAsync({
          depotId: activeDepotId,
          name,
        });
      } else if (createState?.kind === "aisle") {
        await createStorageAisle.mutateAsync({
          storageZoneId: createState.storageZoneId,
          name,
        });
      } else if (createState?.kind === "bin") {
        await createBinLocation.mutateAsync({
          storageAisleId: createState.storageAisleId,
          name,
          isActive: draftIsActive,
          deliveryZoneId: draftDeliveryZoneId || null,
        });
      }

      resetDrafts();
    } catch (error) {
      setSubmitError(getErrorMessage(error));
    }
  }

  async function handleEditSubmit(event: React.FormEvent) {
    event.preventDefault();

    const name = draftName.trim();
    if (!name) {
      setSubmitError("Name is required.");
      return;
    }

    try {
      if (editState?.kind === "zone") {
        await updateStorageZone.mutateAsync({
          id: editState.zone.id,
          data: {
            name,
          },
        });
      } else if (editState?.kind === "aisle") {
        await updateStorageAisle.mutateAsync({
          id: editState.aisle.id,
          data: {
            name,
          },
        });
      } else if (editState?.kind === "bin") {
        await updateBinLocation.mutateAsync({
          id: editState.bin.id,
          data: {
            name,
            isActive: draftIsActive,
            deliveryZoneId: draftDeliveryZoneId || null,
          },
        });
      }

      resetDrafts();
    } catch (error) {
      setSubmitError(getErrorMessage(error));
    }
  }

  async function confirmDelete() {
    if (!deleteState) {
      return;
    }

    try {
      if (deleteState.kind === "zone") {
        await deleteStorageZone.mutateAsync(deleteState.id);
      } else if (deleteState.kind === "aisle") {
        await deleteStorageAisle.mutateAsync(deleteState.id);
      } else {
        await deleteBinLocation.mutateAsync(deleteState.id);
      }

      setDeleteState(null);
      setSubmitError(undefined);
    } catch (error) {
      setSubmitError(getErrorMessage(error));
    }
  }

  if (isLoading) {
    return <ListPageLoading />;
  }

  if (queryError) {
    return (
      <QueryErrorAlert
        title="Could not load bin locations"
        message={getErrorMessage(queryError)}
      />
    );
  }

  if (!depots?.length) {
    return (
      <>
        <ListPageHeader
          variant="vehicle"
          eyebrow="Warehouse"
          title="Bin Locations"
          description="Organize depot storage zones, aisles, and bins for warehouse operations."
          icon={<Boxes strokeWidth={1.75} aria-hidden />}
        />
        <div className="rounded-2xl border border-dashed border-border p-12 text-center">
          <p className="font-medium">No depots available</p>
          <p className="mt-1 text-sm text-muted-foreground">
            Create a depot first before managing storage zones and bins.
          </p>
        </div>
      </>
    );
  }

  const isBusy =
    createStorageZone.isPending
    || updateStorageZone.isPending
    || deleteStorageZone.isPending
    || createStorageAisle.isPending
    || updateStorageAisle.isPending
    || deleteStorageAisle.isPending
    || createBinLocation.isPending
    || updateBinLocation.isPending
    || deleteBinLocation.isPending;

  return (
    <>
      <BinLocationDeleteDialog
        deleteState={deleteState}
        isBusy={isBusy}
        submitError={submitError}
        onClose={() => setDeleteState(null)}
        onConfirm={() => void confirmDelete()}
      />

      <ListPageHeader
        variant="vehicle"
        eyebrow="Warehouse"
        title="Bin Locations"
        description="Manage depot storage zones, aisles, and bins without affecting delivery coverage zones."
        icon={<Boxes strokeWidth={1.75} aria-hidden />}
        action={(
          <Button onClick={startCreateZone} className="gap-2">
            <Plus className="size-4" aria-hidden />
            Add storage zone
          </Button>
        )}
      />

      <ListPageStatsStrip
        totalLabel="Storage zones"
        totalCount={storageZones.length}
        rangeEntityLabel="storage zones"
        from={storageZones.length ? 1 : 0}
        to={storageZones.length}
        page={1}
        totalPages={1}
        pageSize={Math.max(storageZones.length, 1)}
        filterCardLabel="Depot"
        filterCardHint={`${totalAisles} aisles and ${totalBins} bins in the selected depot`}
        activeFilterDisplay={layout?.depotName ?? "No depot selected"}
      />

      <div className="mb-6 rounded-2xl border border-border/50 bg-card/80 p-5 shadow-[0_1px_0_0_oklch(0_0_0/0.05),0_16px_48px_-20px_oklch(0.4_0.02_250/0.14)] dark:bg-card/60">
        <Label htmlFor="bin-location-depot">Depot</Label>
        <select
          id="bin-location-depot"
          aria-label="Depot"
          value={activeDepotId}
          onChange={(event) => {
            setSelectedDepotId(event.target.value);
            setCollapsedZoneIds(new Set());
            resetDrafts();
          }}
          className="mt-1 flex h-10 w-full items-center rounded-xl border border-input/90 bg-background px-3 py-2 text-sm"
        >
          {depots.map((depot) => (
            <option key={depot.id} value={depot.id}>
              {depot.name}
            </option>
          ))}
        </select>
      </div>

      {createState?.kind === "zone" ? (
        <form onSubmit={handleCreateSubmit} className="mb-6">
          <NameForm
            id="storage-zone-name"
            label="Storage zone name"
            submitLabel={isBusy ? "Creating..." : "Create storage zone"}
            value={draftName}
            onChange={setDraftName}
            onCancel={resetDrafts}
            isPending={isBusy}
          />
          {submitError ? (
            <p className="mt-3 text-sm text-destructive">{submitError}</p>
          ) : null}
        </form>
      ) : null}

      {storageZones.length ? (
        <BinLocationsTree
          storageZones={storageZones}
          collapsedZoneIds={collapsedZoneIds}
          createState={createState}
          editState={editState}
          draftName={draftName}
          draftIsActive={draftIsActive}
          draftDeliveryZoneId={draftDeliveryZoneId}
          deliveryZoneOptions={deliveryZoneOptions}
          disabledDeliveryZoneIds={disabledDeliveryZoneIds}
          submitError={submitError}
          isBusy={isBusy}
          onToggleZone={toggleZone}
          onStartCreateAisle={startCreateAisle}
          onStartCreateBin={startCreateBin}
          onStartEditZone={startEditZone}
          onStartEditAisle={startEditAisle}
          onStartEditBin={startEditBin}
          onDeleteRequested={setDeleteState}
          onCreateSubmit={handleCreateSubmit}
          onEditSubmit={handleEditSubmit}
          onDraftNameChange={setDraftName}
          onDraftIsActiveChange={setDraftIsActive}
          onDraftDeliveryZoneIdChange={setDraftDeliveryZoneId}
          onResetDrafts={resetDrafts}
        />
      ) : (
        <div className="rounded-2xl border border-dashed border-border p-12 text-center">
          <p className="font-medium">No storage zones yet</p>
          <p className="mt-1 text-sm text-muted-foreground">
            Use the Add storage zone action to create the first warehouse area for this depot.
          </p>
        </div>
      )}

      {submitError && !createState && !editState && !deleteState ? (
        <p className="mt-4 text-sm text-destructive">{submitError}</p>
      ) : null}
    </>
  );
}
