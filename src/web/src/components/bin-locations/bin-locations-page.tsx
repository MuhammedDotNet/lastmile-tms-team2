"use client";

import { useEffect, useRef, useState } from "react";
import {
  Boxes,
  ChevronDown,
  ChevronRight,
  Pencil,
  Plus,
  Trash2,
} from "lucide-react";
import { useSession } from "next-auth/react";
import {
  ListPageHeader,
  ListPageLoading,
  ListPageStatsStrip,
} from "@/components/list";
import { QueryErrorAlert } from "@/components/feedback/query-error-alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import { cn } from "@/lib/utils";
import type {
  BinLocation,
  StorageAisle,
  StorageZone,
} from "@/types/bin-locations";

type CreateState =
  | { kind: "zone" }
  | { kind: "aisle"; storageZoneId: string }
  | { kind: "bin"; storageAisleId: string }
  | null;

type EditState =
  | { kind: "zone"; zone: StorageZone }
  | { kind: "aisle"; aisle: StorageAisle }
  | { kind: "bin"; bin: BinLocation }
  | null;

type DeleteState =
  | { kind: "zone"; id: string; name: string }
  | { kind: "aisle"; id: string; name: string }
  | { kind: "bin"; id: string; name: string }
  | null;

function getDialogFocusableElements(container: HTMLElement | null): HTMLElement[] {
  if (!container) {
    return [];
  }

  return Array.from(
    container.querySelectorAll<HTMLElement>(
      'button:not([disabled]), [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])',
    ),
  );
}

function NameForm({
  id,
  label,
  submitLabel,
  value,
  onChange,
  onCancel,
  isPending,
}: {
  id: string;
  label: string;
  submitLabel: string;
  value: string;
  onChange: (value: string) => void;
  onCancel: () => void;
  isPending: boolean;
}) {
  return (
    <div className="rounded-2xl border border-border/60 bg-background/80 p-4">
      <div className="space-y-3">
        <div>
          <Label htmlFor={id}>{label}</Label>
          <Input
            id={id}
            value={value}
            onChange={(event) => onChange(event.target.value)}
            required
          />
        </div>
        <div className="flex justify-end gap-3">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isPending}>
            {submitLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}

function BinForm({
  value,
  isActive,
  onNameChange,
  onActiveChange,
  onCancel,
  submitLabel,
  isPending,
}: {
  value: string;
  isActive: boolean;
  onNameChange: (value: string) => void;
  onActiveChange: (value: boolean) => void;
  onCancel: () => void;
  submitLabel: string;
  isPending: boolean;
}) {
  return (
    <div className="rounded-2xl border border-border/60 bg-background/80 p-4">
      <div className="grid gap-3 sm:grid-cols-2">
        <div>
          <Label htmlFor="bin-name">Bin name</Label>
          <Input
            id="bin-name"
            value={value}
            onChange={(event) => onNameChange(event.target.value)}
            required
          />
        </div>
        <div>
          <Label htmlFor="bin-status">Status</Label>
          <select
            id="bin-status"
            value={isActive ? "true" : "false"}
            onChange={(event) => onActiveChange(event.target.value === "true")}
            className="flex h-10 w-full items-center rounded-xl border border-input/90 bg-background px-3 py-2 text-sm"
          >
            <option value="true">Active</option>
            <option value="false">Inactive</option>
          </select>
        </div>
      </div>
      <div className="mt-4 flex justify-end gap-3">
        <Button type="button" variant="outline" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" disabled={isPending}>
          {submitLabel}
        </Button>
      </div>
    </div>
  );
}

export default function BinLocationsPage() {
  const { status } = useSession();
  const { data: depots, isLoading: depotsLoading, error: depotsError } = useDepots();
  const deleteDialogRef = useRef<HTMLDivElement | null>(null);
  const [selectedDepotId, setSelectedDepotId] = useState("");
  const [collapsedZoneIds, setCollapsedZoneIds] = useState<Set<string>>(new Set());
  const [createState, setCreateState] = useState<CreateState>(null);
  const [editState, setEditState] = useState<EditState>(null);
  const [deleteState, setDeleteState] = useState<DeleteState>(null);
  const [draftName, setDraftName] = useState("");
  const [draftIsActive, setDraftIsActive] = useState(true);
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

  const queryError = depotsError ?? layoutQuery.error;
  const isLoading =
    status === "loading"
    || depotsLoading
    || (Boolean(activeDepotId) && layoutQuery.isLoading);

  let totalAisles = 0;
  let totalBins = 0;
  for (const zone of layout?.storageZones ?? []) {
    totalAisles += zone.storageAisles.length;
    for (const aisle of zone.storageAisles) {
      totalBins += aisle.binLocations.length;
    }
  }

  function resetDrafts() {
    setCreateState(null);
    setEditState(null);
    setDraftName("");
    setDraftIsActive(true);
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
    setSubmitError(undefined);
  }

  function startCreateAisle(storageZoneId: string) {
    setCreateState({ kind: "aisle", storageZoneId });
    setEditState(null);
    setDraftName("");
    setDraftIsActive(true);
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
    setSubmitError(undefined);
  }

  function startEditZone(zone: StorageZone) {
    setEditState({ kind: "zone", zone });
    setCreateState(null);
    setDraftName(zone.name);
    setDraftIsActive(true);
    setSubmitError(undefined);
  }

  function startEditAisle(aisle: StorageAisle) {
    setEditState({ kind: "aisle", aisle });
    setCreateState(null);
    setDraftName(aisle.name);
    setDraftIsActive(true);
    setSubmitError(undefined);
  }

  function startEditBin(bin: BinLocation) {
    setEditState({ kind: "bin", bin });
    setCreateState(null);
    setDraftName(bin.name);
    setDraftIsActive(bin.isActive);
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

  useEffect(() => {
    if (!deleteState) {
      return;
    }

    const dialog = deleteDialogRef.current;
    const previousActiveElement =
      document.activeElement instanceof HTMLElement
        ? document.activeElement
        : null;
    const previousOverflow = document.body.style.overflow;

    document.body.style.overflow = "hidden";

    const animationFrameId = window.requestAnimationFrame(() => {
      const focusableElements = getDialogFocusableElements(dialog);
      if (focusableElements.length) {
        focusableElements[0].focus();
      } else {
        dialog?.focus();
      }
    });

    function handleKeyDown(event: KeyboardEvent) {
      if (!deleteDialogRef.current) {
        return;
      }

      if (event.key === "Escape") {
        event.preventDefault();
        setDeleteState(null);
        return;
      }

      if (event.key !== "Tab") {
        return;
      }

      const focusableElements = getDialogFocusableElements(deleteDialogRef.current);
      if (!focusableElements.length) {
        event.preventDefault();
        deleteDialogRef.current.focus();
        return;
      }

      const firstElement = focusableElements[0];
      const lastElement = focusableElements[focusableElements.length - 1];
      const activeElement = document.activeElement;

      if (event.shiftKey) {
        if (activeElement === firstElement || activeElement === deleteDialogRef.current) {
          event.preventDefault();
          lastElement.focus();
        }
        return;
      }

      if (activeElement === lastElement) {
        event.preventDefault();
        firstElement.focus();
      }
    }

    document.addEventListener("keydown", handleKeyDown);

    return () => {
      window.cancelAnimationFrame(animationFrameId);
      document.removeEventListener("keydown", handleKeyDown);
      document.body.style.overflow = previousOverflow;
      previousActiveElement?.focus();
    };
  }, [deleteState]);

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
      {deleteState ? (
        <div
          className="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/40 p-4"
          onMouseDown={(event) => {
            if (event.target === event.currentTarget) {
              setDeleteState(null);
            }
          }}
        >
          <div
            ref={deleteDialogRef}
            role="dialog"
            aria-modal="true"
            aria-labelledby="bin-location-delete-title"
            aria-describedby="bin-location-delete-description"
            tabIndex={-1}
            className="mt-[15vh] w-full max-w-sm rounded-2xl border border-border bg-card p-6 shadow-xl"
          >
            <h2 id="bin-location-delete-title" className="mb-2 text-lg font-semibold">
              Delete {deleteState.kind === "zone" ? "storage zone" : deleteState.kind === "aisle" ? "storage aisle" : "bin"}
            </h2>
            <p id="bin-location-delete-description" className="mb-6 text-sm text-muted-foreground">
              Are you sure you want to delete <strong>{deleteState.name}</strong>?
            </p>
            <div className="flex justify-end gap-3">
              <Button variant="outline" onClick={() => setDeleteState(null)}>
                Cancel
              </Button>
              <Button variant="destructive" disabled={isBusy} onClick={() => void confirmDelete()}>
                {isBusy ? "Deleting..." : "Delete"}
              </Button>
            </div>
            {submitError ? (
              <p className="mt-3 text-sm text-destructive">{submitError}</p>
            ) : null}
          </div>
        </div>
      ) : null}

      <ListPageHeader
        variant="vehicle"
        eyebrow="Warehouse"
        title="Bin Locations"
        description="Manage depot storage zones, aisles, and bins without affecting delivery coverage zones."
        icon={<Boxes strokeWidth={1.75} aria-hidden />}
        action={
          <Button onClick={startCreateZone} className="gap-2">
            <Plus className="size-4" aria-hidden />
            Add storage zone
          </Button>
        }
      />

      <ListPageStatsStrip
        totalLabel="Storage zones"
        totalCount={layout?.storageZones.length ?? 0}
        rangeEntityLabel="storage zones"
        from={layout?.storageZones.length ? 1 : 0}
        to={layout?.storageZones.length ?? 0}
        page={1}
        totalPages={1}
        pageSize={Math.max(layout?.storageZones.length ?? 0, 1)}
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

      {layout?.storageZones.length ? (
        <div className="space-y-4">
          {layout.storageZones.map((zone) => {
            const isExpanded = !collapsedZoneIds.has(zone.id);

            return (
              <section
                key={zone.id}
                className="rounded-2xl border border-border/50 bg-card/80 p-5 shadow-[0_1px_0_0_oklch(0_0_0/0.05),0_16px_48px_-20px_oklch(0.4_0.02_250/0.14)] dark:bg-card/60"
              >
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <button
                    type="button"
                    onClick={() => toggleZone(zone.id)}
                    className="flex items-center gap-2 text-left"
                  >
                    {isExpanded ? (
                      <ChevronDown className="size-4 text-muted-foreground" aria-hidden />
                    ) : (
                      <ChevronRight className="size-4 text-muted-foreground" aria-hidden />
                    )}
                    <div>
                      <p className="font-semibold">{zone.name}</p>
                      <p className="text-sm text-muted-foreground">
                        {zone.storageAisles.length} aisle{zone.storageAisles.length === 1 ? "" : "s"}
                      </p>
                    </div>
                  </button>
                  <div className="flex flex-wrap justify-end gap-2">
                    <Button type="button" variant="outline" size="sm" onClick={() => startCreateAisle(zone.id)}>
                      <Plus className="size-3.5" aria-hidden />
                      Add aisle
                    </Button>
                    <Button type="button" variant="outline" size="sm" onClick={() => startEditZone(zone)}>
                      <Pencil className="size-3.5" aria-hidden />
                      Edit
                    </Button>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      className="border-destructive/25 text-destructive hover:bg-destructive/10 hover:text-destructive"
                      onClick={() => setDeleteState({ kind: "zone", id: zone.id, name: zone.name })}
                    >
                      <Trash2 className="size-3.5" aria-hidden />
                      Delete
                    </Button>
                  </div>
                </div>

                {editState?.kind === "zone" && editState.zone.id === zone.id ? (
                  <form onSubmit={handleEditSubmit} className="mt-4">
                    <NameForm
                      id={`edit-zone-${zone.id}`}
                      label="Storage zone name"
                      submitLabel={isBusy ? "Saving..." : "Save storage zone"}
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

                {createState?.kind === "aisle" && createState.storageZoneId === zone.id ? (
                  <form onSubmit={handleCreateSubmit} className="mt-4">
                    <NameForm
                      id={`create-aisle-${zone.id}`}
                      label="Storage aisle name"
                      submitLabel={isBusy ? "Creating..." : "Create storage aisle"}
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

                {isExpanded ? (
                  <div className="mt-4 space-y-3 border-t border-border/50 pt-4">
                    {zone.storageAisles.length ? (
                      zone.storageAisles.map((aisle) => (
                        <article
                          key={aisle.id}
                          className="rounded-2xl border border-border/50 bg-background/70 p-4"
                        >
                          <div className="flex flex-wrap items-center justify-between gap-3">
                            <div>
                              <p className="font-medium">{aisle.name}</p>
                              <p className="text-sm text-muted-foreground">
                                {aisle.binLocations.length} bin{aisle.binLocations.length === 1 ? "" : "s"}
                              </p>
                            </div>
                            <div className="flex flex-wrap justify-end gap-2">
                              <Button type="button" variant="outline" size="sm" onClick={() => startCreateBin(aisle.id)}>
                                <Plus className="size-3.5" aria-hidden />
                                Add bin
                              </Button>
                              <Button type="button" variant="outline" size="sm" onClick={() => startEditAisle(aisle)}>
                                <Pencil className="size-3.5" aria-hidden />
                                Edit
                              </Button>
                              <Button
                                type="button"
                                variant="outline"
                                size="sm"
                                className="border-destructive/25 text-destructive hover:bg-destructive/10 hover:text-destructive"
                                onClick={() =>
                                  setDeleteState({ kind: "aisle", id: aisle.id, name: aisle.name })
                                }
                              >
                                <Trash2 className="size-3.5" aria-hidden />
                                Delete
                              </Button>
                            </div>
                          </div>

                          {editState?.kind === "aisle" && editState.aisle.id === aisle.id ? (
                            <form onSubmit={handleEditSubmit} className="mt-4">
                              <NameForm
                                id={`edit-aisle-${aisle.id}`}
                                label="Storage aisle name"
                                submitLabel={isBusy ? "Saving..." : "Save storage aisle"}
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

                          {createState?.kind === "bin" && createState.storageAisleId === aisle.id ? (
                            <form onSubmit={handleCreateSubmit} className="mt-4">
                              <BinForm
                                value={draftName}
                                isActive={draftIsActive}
                                onNameChange={setDraftName}
                                onActiveChange={setDraftIsActive}
                                onCancel={resetDrafts}
                                submitLabel={isBusy ? "Creating..." : "Create bin location"}
                                isPending={isBusy}
                              />
                              {submitError ? (
                                <p className="mt-3 text-sm text-destructive">{submitError}</p>
                              ) : null}
                            </form>
                          ) : null}

                          <div className="mt-4 space-y-2">
                            {aisle.binLocations.length ? (
                              aisle.binLocations.map((bin) => (
                                <div
                                  key={bin.id}
                                  className="rounded-xl border border-border/50 bg-card/70 p-3"
                                >
                                  <div className="flex flex-wrap items-center justify-between gap-3">
                                    <div className="flex items-center gap-3">
                                      <span className="font-medium">{bin.name}</span>
                                      <span
                                        className={cn(
                                          "inline-flex rounded-full px-2.5 py-1 text-xs font-medium",
                                          bin.isActive
                                            ? "bg-emerald-100 text-emerald-700"
                                            : "bg-amber-100 text-amber-700",
                                        )}
                                      >
                                        {bin.isActive ? "Active" : "Inactive"}
                                      </span>
                                    </div>
                                    <div className="flex flex-wrap justify-end gap-2">
                                      <Button type="button" variant="outline" size="sm" onClick={() => startEditBin(bin)}>
                                        <Pencil className="size-3.5" aria-hidden />
                                        Edit
                                      </Button>
                                      <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        className="border-destructive/25 text-destructive hover:bg-destructive/10 hover:text-destructive"
                                        onClick={() =>
                                          setDeleteState({ kind: "bin", id: bin.id, name: bin.name })
                                        }
                                      >
                                        <Trash2 className="size-3.5" aria-hidden />
                                        Delete
                                      </Button>
                                    </div>
                                  </div>

                                  {editState?.kind === "bin" && editState.bin.id === bin.id ? (
                                    <form onSubmit={handleEditSubmit} className="mt-4">
                                      <BinForm
                                        value={draftName}
                                        isActive={draftIsActive}
                                        onNameChange={setDraftName}
                                        onActiveChange={setDraftIsActive}
                                        onCancel={resetDrafts}
                                        submitLabel={isBusy ? "Saving..." : "Save bin location"}
                                        isPending={isBusy}
                                      />
                                      {submitError ? (
                                        <p className="mt-3 text-sm text-destructive">{submitError}</p>
                                      ) : null}
                                    </form>
                                  ) : null}
                                </div>
                              ))
                            ) : (
                              <p className="text-sm text-muted-foreground">No bins in this aisle yet.</p>
                            )}
                          </div>
                        </article>
                      ))
                    ) : (
                      <p className="text-sm text-muted-foreground">No aisles in this storage zone yet.</p>
                    )}
                  </div>
                ) : null}
              </section>
            );
          })}
        </div>
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
