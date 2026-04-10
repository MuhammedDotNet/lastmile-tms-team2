import { cn } from "@/lib/utils";
import type { RouteStatus, StagingArea } from "@/types/routes";

export const ROUTE_STATUS_LABELS: Record<RouteStatus, string> = {
  CANCELLED: "Cancelled",
  COMPLETED: "Completed",
  DISPATCHED: "Dispatched",
  DRAFT: "Draft",
  IN_PROGRESS: "In Progress",
};

export const ROUTE_STATUS_ORDER: RouteStatus[] = [
  "DRAFT",
  "DISPATCHED",
  "IN_PROGRESS",
  "COMPLETED",
  "CANCELLED",
];

export const ROUTE_ASSIGNMENT_MODE_LABELS = {
  MANUAL_PARCELS: "Manual parcels",
  AUTO_BY_ZONE: "Auto by zone",
} as const;

export const ROUTE_STOP_MODE_LABELS = {
  AUTO: "Auto curbside stops",
  MANUAL: "Manual stop editing",
} as const;

export const STAGING_AREA_LABELS: Record<StagingArea, string> = {
  A: "Area A",
  B: "Area B",
};

const routeStatusBadgeBase =
  "inline-flex max-w-full min-w-0 items-center truncate rounded-full px-2 py-0.5 text-xs font-medium";

export function routeStatusBadgeClass(status: string): string {
  switch (status) {
    case "DRAFT":
      return cn(routeStatusBadgeBase, "bg-muted text-muted-foreground");
    case "DISPATCHED":
      return cn(
        routeStatusBadgeBase,
        "bg-amber-100 text-amber-900 dark:bg-amber-950/50 dark:text-amber-200",
      );
    case "IN_PROGRESS":
      return cn(
        routeStatusBadgeBase,
        "bg-blue-100 text-blue-800 dark:bg-blue-950/50 dark:text-blue-200",
      );
    case "COMPLETED":
      return cn(
        routeStatusBadgeBase,
        "bg-emerald-100 text-emerald-900 dark:bg-emerald-950/50 dark:text-emerald-200",
      );
    case "CANCELLED":
      return cn(
        routeStatusBadgeBase,
        "bg-red-100 text-red-900 dark:bg-red-950/50 dark:text-red-200",
      );
    default:
      return routeStatusBadgeBase;
  }
}
