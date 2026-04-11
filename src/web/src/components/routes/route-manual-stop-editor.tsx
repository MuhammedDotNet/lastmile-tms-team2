"use client";

import { useMemo } from "react";
import { Button } from "@/components/ui/button";
import { SelectDropdown } from "@/components/form/select-dropdown";
import type { RoutePlanCandidateParcel } from "@/types/routes";

export const NEW_STOP_VALUE = "__new_stop__";

export type ManualStopState = {
  id: string;
  parcelIds: string[];
};

export function createManualStopId(seed?: string) {
  return seed
    ? `stop-${seed}`
    : `stop-${Math.random().toString(36).slice(2, 10)}`;
}

export function normalizeManualStops(stops: ManualStopState[]) {
  return stops
    .map((stop) => ({
      ...stop,
      parcelIds: Array.from(new Set(stop.parcelIds.filter(Boolean))),
    }))
    .filter((stop) => stop.parcelIds.length > 0);
}

export function sameManualStops(left: ManualStopState[], right: ManualStopState[]) {
  return JSON.stringify(left) === JSON.stringify(right);
}

export function reconcileManualStops(
  stops: ManualStopState[],
  selectedParcelIds: string[],
): ManualStopState[] {
  const selectedSet = new Set(selectedParcelIds);
  const next = normalizeManualStops(
    stops.map((stop) => ({
      ...stop,
      parcelIds: stop.parcelIds.filter((parcelId) => selectedSet.has(parcelId)),
    })),
  );

  const assigned = new Set(next.flatMap((stop) => stop.parcelIds));
  for (const parcelId of selectedParcelIds) {
    if (!assigned.has(parcelId)) {
      next.push({ id: createManualStopId(parcelId), parcelIds: [parcelId] });
    }
  }

  return next;
}

export function moveStop(stops: ManualStopState[], index: number, direction: -1 | 1) {
  const nextIndex = index + direction;
  if (nextIndex < 0 || nextIndex >= stops.length) {
    return stops;
  }

  const next = [...stops];
  [next[index], next[nextIndex]] = [next[nextIndex], next[index]];
  return next;
}

export function moveParcelToStop(
  stops: ManualStopState[],
  parcelId: string,
  sourceStopId: string,
  targetStopId: string,
): ManualStopState[] {
  if (targetStopId === sourceStopId) {
    return stops;
  }

  const next = normalizeManualStops(
    stops.map((stop) => ({
      ...stop,
      parcelIds: stop.parcelIds.filter((candidate) => candidate !== parcelId),
    })),
  );
  const sourceIndex = stops.findIndex((stop) => stop.id === sourceStopId);

  if (targetStopId === NEW_STOP_VALUE) {
    const insertIndex =
      sourceIndex >= 0 ? Math.min(sourceIndex + 1, next.length) : next.length;
    next.splice(insertIndex, 0, {
      id: createManualStopId(),
      parcelIds: [parcelId],
    });
    return next;
  }

  const targetStop = next.find((stop) => stop.id === targetStopId);
  if (!targetStop) {
    return stops;
  }

  targetStop.parcelIds.push(parcelId);
  return normalizeManualStops(next);
}

export function RouteManualStopEditor({
  stops,
  parcelsById,
  onChange,
}: {
  stops: ManualStopState[];
  parcelsById: Map<string, RoutePlanCandidateParcel>;
  onChange: (stops: ManualStopState[]) => void;
}) {
  const stopOptions = useMemo(
    () => [
      ...stops.map((stop, index) => ({
        value: stop.id,
        label: `Stop ${index + 1}`,
      })),
      { value: NEW_STOP_VALUE, label: "Create new stop" },
    ],
    [stops],
  );

  return (
    <div className="space-y-4">
      {stops.map((stop, index) => (
        <div
          key={stop.id}
          className="rounded-2xl border border-border/60 bg-background/60 p-4"
        >
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <p className="text-sm font-semibold">Stop {index + 1}</p>
              <p className="text-xs text-muted-foreground">
                Merge same-drop parcels or split them into separate stops.
              </p>
            </div>
            <div className="flex gap-2">
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => onChange(moveStop(stops, index, -1))}
                disabled={index === 0}
              >
                Up
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={() => onChange(moveStop(stops, index, 1))}
                disabled={index === stops.length - 1}
              >
                Down
              </Button>
            </div>
          </div>

          <div className="mt-4 space-y-3">
            {stop.parcelIds.map((parcelId) => {
              const parcel = parcelsById.get(parcelId);
              if (!parcel) {
                return null;
              }

              return (
                <div
                  key={parcelId}
                  className="rounded-xl border border-border/50 bg-muted/20 p-3"
                >
                  <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium">
                        {parcel.trackingNumber} | {parcel.recipientLabel}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        {parcel.addressLine}
                      </p>
                    </div>
                    <div className="w-full max-w-[14rem]">
                      <SelectDropdown
                        options={stopOptions}
                        value={stop.id}
                        onChange={(value) =>
                          onChange(moveParcelToStop(stops, parcelId, stop.id, value))
                        }
                      />
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      ))}
    </div>
  );
}

export default RouteManualStopEditor;
