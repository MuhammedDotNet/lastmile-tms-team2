import { cn } from "@/lib/utils";
import { RouteStatus } from "@/types/routes";

export const ROUTE_STATUS_LABELS: Record<RouteStatus, string> = {
  [RouteStatus.Planned]: "Planned",
  [RouteStatus.InProgress]: "In Progress",
  [RouteStatus.Completed]: "Completed",
  [RouteStatus.Cancelled]: "Cancelled",
};

export const ROUTE_STATUS_ORDER: RouteStatus[] = [
  RouteStatus.Planned,
  RouteStatus.InProgress,
  RouteStatus.Completed,
  RouteStatus.Cancelled,
];

const routeStatusBadgeBase =
  "inline-flex max-w-full min-w-0 items-center truncate rounded-full px-2 py-0.5 text-xs font-medium";

export function routeStatusBadgeClass(status: RouteStatus): string {
  switch (status) {
    case RouteStatus.Planned:
      return cn(routeStatusBadgeBase, "bg-muted text-muted-foreground");
    case RouteStatus.InProgress:
      return cn(
        routeStatusBadgeBase,
        "bg-blue-100 text-blue-800 dark:bg-blue-950/50 dark:text-blue-200",
      );
    case RouteStatus.Completed:
      return cn(
        routeStatusBadgeBase,
        "bg-emerald-100 text-emerald-900 dark:bg-emerald-950/50 dark:text-emerald-200",
      );
    case RouteStatus.Cancelled:
      return cn(
        routeStatusBadgeBase,
        "bg-red-100 text-red-900 dark:bg-red-950/50 dark:text-red-200",
      );
    default:
      return routeStatusBadgeBase;
  }
}
