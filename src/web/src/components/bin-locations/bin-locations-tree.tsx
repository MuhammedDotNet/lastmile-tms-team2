import type { FormEvent } from "react";
import {
  ChevronDown,
  ChevronRight,
  Pencil,
  Plus,
  Trash2,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import {
  BinForm,
  NameForm,
} from "@/components/bin-locations/bin-location-forms";
import type {
  CreateState,
  DeleteState,
  EditState,
} from "@/components/bin-locations/bin-locations-state";
import type {
  DeliveryZoneOption,
  StorageZone,
} from "@/types/bin-locations";

interface BinLocationsTreeProps {
  storageZones: StorageZone[];
  collapsedZoneIds: Set<string>;
  createState: CreateState;
  editState: EditState;
  draftName: string;
  draftIsActive: boolean;
  draftDeliveryZoneId: string;
  deliveryZoneOptions: DeliveryZoneOption[];
  disabledDeliveryZoneIds: Set<string>;
  submitError?: string;
  isBusy: boolean;
  onToggleZone: (zoneId: string) => void;
  onStartCreateAisle: (storageZoneId: string) => void;
  onStartCreateBin: (storageAisleId: string) => void;
  onStartEditZone: (zone: StorageZone) => void;
  onStartEditAisle: (aisleId: string) => void;
  onStartEditBin: (binId: string) => void;
  onDeleteRequested: (state: NonNullable<DeleteState>) => void;
  onCreateSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onEditSubmit: (event: FormEvent<HTMLFormElement>) => void;
  onDraftNameChange: (value: string) => void;
  onDraftIsActiveChange: (value: boolean) => void;
  onDraftDeliveryZoneIdChange: (value: string) => void;
  onResetDrafts: () => void;
}

export function BinLocationsTree({
  storageZones,
  collapsedZoneIds,
  createState,
  editState,
  draftName,
  draftIsActive,
  draftDeliveryZoneId,
  deliveryZoneOptions,
  disabledDeliveryZoneIds,
  submitError,
  isBusy,
  onToggleZone,
  onStartCreateAisle,
  onStartCreateBin,
  onStartEditZone,
  onStartEditAisle,
  onStartEditBin,
  onDeleteRequested,
  onCreateSubmit,
  onEditSubmit,
  onDraftNameChange,
  onDraftIsActiveChange,
  onDraftDeliveryZoneIdChange,
  onResetDrafts,
}: BinLocationsTreeProps) {
  return (
    <div className="space-y-4">
      {storageZones.map((zone) => {
        const isExpanded = !collapsedZoneIds.has(zone.id);

        return (
          <section
            key={zone.id}
            className="rounded-2xl border border-border/50 bg-card/80 p-5 shadow-[0_1px_0_0_oklch(0_0_0/0.05),0_16px_48px_-20px_oklch(0.4_0.02_250/0.14)] dark:bg-card/60"
          >
            <div className="flex flex-wrap items-center justify-between gap-3">
              <button
                type="button"
                onClick={() => onToggleZone(zone.id)}
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
                <Button type="button" variant="outline" size="sm" onClick={() => onStartCreateAisle(zone.id)}>
                  <Plus className="size-3.5" aria-hidden />
                  Add aisle
                </Button>
                <Button type="button" variant="outline" size="sm" onClick={() => onStartEditZone(zone)}>
                  <Pencil className="size-3.5" aria-hidden />
                  Edit
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="border-destructive/25 text-destructive hover:bg-destructive/10 hover:text-destructive"
                  onClick={() => onDeleteRequested({ kind: "zone", id: zone.id, name: zone.name })}
                >
                  <Trash2 className="size-3.5" aria-hidden />
                  Delete
                </Button>
              </div>
            </div>

            {editState?.kind === "zone" && editState.zone.id === zone.id ? (
              <form onSubmit={onEditSubmit} className="mt-4">
                <NameForm
                  id={`edit-zone-${zone.id}`}
                  label="Storage zone name"
                  submitLabel={isBusy ? "Saving..." : "Save storage zone"}
                  value={draftName}
                  onChange={onDraftNameChange}
                  onCancel={onResetDrafts}
                  isPending={isBusy}
                />
                {submitError ? (
                  <p className="mt-3 text-sm text-destructive">{submitError}</p>
                ) : null}
              </form>
            ) : null}

            {createState?.kind === "aisle" && createState.storageZoneId === zone.id ? (
              <form onSubmit={onCreateSubmit} className="mt-4">
                <NameForm
                  id={`create-aisle-${zone.id}`}
                  label="Storage aisle name"
                  submitLabel={isBusy ? "Creating..." : "Create storage aisle"}
                  value={draftName}
                  onChange={onDraftNameChange}
                  onCancel={onResetDrafts}
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
                          <Button type="button" variant="outline" size="sm" onClick={() => onStartCreateBin(aisle.id)}>
                            <Plus className="size-3.5" aria-hidden />
                            Add bin
                          </Button>
                          <Button type="button" variant="outline" size="sm" onClick={() => onStartEditAisle(aisle.id)}>
                            <Pencil className="size-3.5" aria-hidden />
                            Edit
                          </Button>
                          <Button
                            type="button"
                            variant="outline"
                            size="sm"
                            className="border-destructive/25 text-destructive hover:bg-destructive/10 hover:text-destructive"
                            onClick={() =>
                              onDeleteRequested({ kind: "aisle", id: aisle.id, name: aisle.name })
                            }
                          >
                            <Trash2 className="size-3.5" aria-hidden />
                            Delete
                          </Button>
                        </div>
                      </div>

                      {editState?.kind === "aisle" && editState.aisle.id === aisle.id ? (
                        <form onSubmit={onEditSubmit} className="mt-4">
                          <NameForm
                            id={`edit-aisle-${aisle.id}`}
                            label="Storage aisle name"
                            submitLabel={isBusy ? "Saving..." : "Save storage aisle"}
                            value={draftName}
                            onChange={onDraftNameChange}
                            onCancel={onResetDrafts}
                            isPending={isBusy}
                          />
                          {submitError ? (
                            <p className="mt-3 text-sm text-destructive">{submitError}</p>
                          ) : null}
                        </form>
                      ) : null}

                      {createState?.kind === "bin" && createState.storageAisleId === aisle.id ? (
                        <form onSubmit={onCreateSubmit} className="mt-4">
                          <BinForm
                            value={draftName}
                            isActive={draftIsActive}
                            deliveryZoneId={draftDeliveryZoneId}
                            deliveryZoneOptions={deliveryZoneOptions}
                            disabledDeliveryZoneIds={disabledDeliveryZoneIds}
                            onNameChange={onDraftNameChange}
                            onActiveChange={onDraftIsActiveChange}
                            onDeliveryZoneChange={onDraftDeliveryZoneIdChange}
                            onCancel={onResetDrafts}
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
                                <div>
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
                                  <p className="mt-1 text-sm text-muted-foreground">
                                    Delivery zone: {bin.deliveryZoneName ?? "Unassigned"}
                                  </p>
                                </div>
                                <div className="flex flex-wrap justify-end gap-2">
                                  <Button type="button" variant="outline" size="sm" onClick={() => onStartEditBin(bin.id)}>
                                    <span className="sr-only">Edit bin {bin.name}</span>
                                    <Pencil className="size-3.5" aria-hidden />
                                    Edit
                                  </Button>
                                  <Button
                                    type="button"
                                    variant="outline"
                                    size="sm"
                                    className="border-destructive/25 text-destructive hover:bg-destructive/10 hover:text-destructive"
                                    onClick={() =>
                                      onDeleteRequested({ kind: "bin", id: bin.id, name: bin.name })
                                    }
                                  >
                                    <span className="sr-only">Delete bin {bin.name}</span>
                                    <Trash2 className="size-3.5" aria-hidden />
                                    Delete
                                  </Button>
                                </div>
                              </div>

                              {editState?.kind === "bin" && editState.bin.id === bin.id ? (
                                <form onSubmit={onEditSubmit} className="mt-4">
                                  <BinForm
                                    value={draftName}
                                    isActive={draftIsActive}
                                    deliveryZoneId={draftDeliveryZoneId}
                                    deliveryZoneOptions={deliveryZoneOptions}
                                    disabledDeliveryZoneIds={disabledDeliveryZoneIds}
                                    onNameChange={onDraftNameChange}
                                    onActiveChange={onDraftIsActiveChange}
                                    onDeliveryZoneChange={onDraftDeliveryZoneIdChange}
                                    onCancel={onResetDrafts}
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
  );
}
