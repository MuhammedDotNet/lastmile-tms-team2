"use client";

import {
  Activity,
  AlertCircle,
  ArrowLeftRight,
  CheckCircle2,
  Clock,
  MapPin,
  Package,
  Truck,
  UserRound,
  XCircle,
} from "lucide-react";

import { DetailPanel } from "@/components/detail";
import { formatParcelStatus } from "@/lib/labels/parcels";
import type { TrackingEvent } from "@/types/parcels";

function getEventIcon(eventType: string) {
  switch (eventType) {
    case "LabelCreated":
      return <Package className="size-4" strokeWidth={1.75} aria-hidden />;
    case "PickedUp":
      return <Truck className="size-4" strokeWidth={1.75} aria-hidden />;
    case "ArrivedAtFacility":
      return <MapPin className="size-4" strokeWidth={1.75} aria-hidden />;
    case "DepartedFacility":
      return <MapPin className="size-4" strokeWidth={1.75} aria-hidden />;
    case "InTransit":
      return <Truck className="size-4" strokeWidth={1.75} aria-hidden />;
    case "OutForDelivery":
      return <Truck className="size-4" strokeWidth={1.75} aria-hidden />;
    case "Delivered":
      return <CheckCircle2 className="size-4" strokeWidth={1.75} aria-hidden />;
    case "DeliveryAttempted":
      return <AlertCircle className="size-4" strokeWidth={1.75} aria-hidden />;
    case "Exception":
      return <XCircle className="size-4" strokeWidth={1.75} aria-hidden />;
    case "Returned":
      return <ArrowLeftRight className="size-4" strokeWidth={1.75} aria-hidden />;
    case "AddressCorrection":
      return <MapPin className="size-4" strokeWidth={1.75} aria-hidden />;
    case "CustomsClearance":
      return <Activity className="size-4" strokeWidth={1.75} aria-hidden />;
    case "HeldAtFacility":
      return <Clock className="size-4" strokeWidth={1.75} aria-hidden />;
    default:
      return <Package className="size-4" strokeWidth={1.75} aria-hidden />;
  }
}

function getEventIconBgClass(eventType: string): string {
  switch (eventType) {
    case "Delivered":
      return "bg-emerald-100 text-emerald-700 ring-emerald-200 dark:bg-emerald-950/50 dark:text-emerald-300 dark:ring-emerald-800";
    case "OutForDelivery":
      return "bg-amber-100 text-amber-700 ring-amber-200 dark:bg-amber-950/50 dark:text-amber-300 dark:ring-amber-800";
    case "Exception":
    case "DeliveryAttempted":
      return "bg-orange-100 text-orange-700 ring-orange-200 dark:bg-orange-950/50 dark:text-orange-300 dark:ring-orange-800";
    case "Returned":
    case "LabelCreated":
      return "bg-blue-100 text-blue-700 ring-blue-200 dark:bg-blue-950/50 dark:text-blue-300 dark:ring-blue-800";
    default:
      return "bg-slate-100 text-slate-700 ring-slate-200 dark:bg-slate-800 dark:text-slate-300 dark:ring-slate-700";
  }
}

function TimelineItem({ event }: { event: TrackingEvent }) {
  return (
    <div className="relative flex gap-4 pb-6 last:pb-0">
      <div className="relative flex shrink-0">
        <div
          className={`flex size-9 items-center justify-center rounded-full ring-1 ${getEventIconBgClass(event.eventType)}`}
        >
          {getEventIcon(event.eventType)}
        </div>
        <div className="absolute left-1/2 top-9 h-full w-px -translate-x-1/2 bg-border" />
      </div>
      <div className="min-w-0 flex-1 pt-1">
        <div className="flex flex-wrap items-baseline justify-between gap-x-3 gap-y-1">
          <p className="text-sm font-medium text-foreground">
            {formatParcelStatus(event.eventType)}
          </p>
          <time className="text-xs tabular-nums text-muted-foreground">
            {new Date(event.timestamp).toLocaleString()}
          </time>
        </div>
        {event.description && (
          <p className="mt-1 text-sm text-muted-foreground">{event.description}</p>
        )}
        <div className="mt-2 flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
          {event.location && (
            <span className="flex items-center gap-1">
              <MapPin className="size-3" aria-hidden />
              {event.location}
            </span>
          )}
          {event.operator && (
            <span className="flex items-center gap-1">
              <UserRound className="size-3" aria-hidden />
              {event.operator}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}

interface ParcelTimelineProps {
  events: TrackingEvent[];
}

export function ParcelTimeline({ events }: ParcelTimelineProps) {
  return (
    <DetailPanel
      title="Status timeline"
      description="Timeline of status changes and location updates for this parcel."
    >
      {events.length === 0 && (
        <p className="py-4 text-sm text-muted-foreground">
          No tracking events recorded yet.
        </p>
      )}
      {events.length > 0 && (
        <div className="space-y-0">
          {events.map((event) => (
            <TimelineItem key={event.id} event={event} />
          ))}
        </div>
      )}
    </DetailPanel>
  );
}
