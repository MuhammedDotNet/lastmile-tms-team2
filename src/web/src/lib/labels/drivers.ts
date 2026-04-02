import { cn } from "@/lib/utils";
import type { DriverStatus } from "@/types/drivers";
import type { DayOfWeek } from "@/graphql/generated";

const driverStatusBadgeBase =
  "inline-flex max-w-full min-w-0 items-center truncate rounded-full px-2 py-0.5 text-xs font-medium";

export function driverStatusBadgeClass(status: string): string {
  switch (status) {
    case "ACTIVE":
      return cn(
        driverStatusBadgeBase,
        "bg-green-100 text-green-800 dark:bg-green-950/50 dark:text-green-200",
      );
    case "INACTIVE":
      return cn(
        driverStatusBadgeBase,
        "bg-gray-100 text-gray-800 dark:bg-gray-950/50 dark:text-gray-200",
      );
    case "ON_LEAVE":
      return cn(
        driverStatusBadgeBase,
        "bg-yellow-100 text-yellow-800 dark:bg-yellow-950/50 dark:text-yellow-200",
      );
    case "SUSPENDED":
      return cn(
        driverStatusBadgeBase,
        "bg-red-100 text-red-800 dark:bg-red-950/50 dark:text-red-200",
      );
    default:
      return driverStatusBadgeBase;
  }
}

export const DRIVER_STATUS_LABELS: Record<DriverStatus, string> = {
  ACTIVE: "Active",
  INACTIVE: "Inactive",
  ON_LEAVE: "On Leave",
  SUSPENDED: "Suspended",
};

const DRIVER_STATUS_ORDER: DriverStatus[] = [
  "ACTIVE",
  "INACTIVE",
  "ON_LEAVE",
  "SUSPENDED",
];

export const DRIVER_STATUS_SELECT_OPTIONS = DRIVER_STATUS_ORDER.map((v) => ({
  value: v,
  label: DRIVER_STATUS_LABELS[v],
}));

export const DAY_OF_WEEK_LABELS: Record<DayOfWeek, string> = {
  SUNDAY: "Sunday",
  MONDAY: "Monday",
  TUESDAY: "Tuesday",
  WEDNESDAY: "Wednesday",
  THURSDAY: "Thursday",
  FRIDAY: "Friday",
  SATURDAY: "Saturday",
};

/** Monday-first week (matches typical work-week UI). */
export const DAY_OF_WEEK_ORDER: DayOfWeek[] = [
  "MONDAY",
  "TUESDAY",
  "WEDNESDAY",
  "THURSDAY",
  "FRIDAY",
  "SATURDAY",
  "SUNDAY",
];

const dayOfWeekRank = new Map<DayOfWeek, number>(
  DAY_OF_WEEK_ORDER.map((d, i) => [d, i]),
);

/** Stable Monday → Sunday order for availability rows from the API. */
export function sortByDayOfWeek<T extends { dayOfWeek: DayOfWeek }>(
  items: T[],
): T[] {
  return [...items].sort(
    (a, b) =>
      (dayOfWeekRank.get(a.dayOfWeek) ?? 99) -
      (dayOfWeekRank.get(b.dayOfWeek) ?? 99),
  );
}

export const DAY_OF_WEEK_SELECT_OPTIONS = DAY_OF_WEEK_ORDER.map((v) => ({
  value: v,
  label: DAY_OF_WEEK_LABELS[v],
}));
