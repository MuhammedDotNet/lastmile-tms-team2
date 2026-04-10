import type { Route } from "@/types/routes";

const V1 = "10000000-0000-0000-0000-000000000001";
const V2 = "10000000-0000-0000-0000-000000000002";

export const mockRoutes: Route[] = [
  {
    id: "20000000-0000-0000-0000-000000000001",
    zoneId: "40000000-0000-0000-0000-000000000001",
    zoneName: "Sydney CBD",
    vehicleId: V1,
    vehiclePlate: "ABC-1234",
    driverId: "30000000-0000-0000-0000-000000000001",
    driverName: "John Smith",
    stagingArea: "A",
    startDate: "2024-03-15T08:00:00Z",
    endDate: "2024-03-15T14:30:00Z",
    startMileage: 45000,
    endMileage: 45120,
    totalMileage: 120,
    status: "COMPLETED",
    parcelCount: 15,
    parcelsDelivered: 15,
    estimatedStopCount: 11,
    plannedDistanceMeters: 34800,
    plannedDurationSeconds: 7200,
    createdAt: "2024-03-15T07:45:00Z",
    updatedAt: null,
    cancellationReason: null,
    path: [],
    stops: [],
    assignmentAuditTrail: [],
  },
  {
    id: "20000000-0000-0000-0000-000000000002",
    zoneId: "40000000-0000-0000-0000-000000000001",
    zoneName: "Sydney CBD",
    vehicleId: V1,
    vehiclePlate: "ABC-1234",
    driverId: "30000000-0000-0000-0000-000000000001",
    driverName: "John Smith",
    stagingArea: "B",
    startDate: "2024-03-18T09:00:00Z",
    endDate: null,
    startMileage: 45120,
    endMileage: 0,
    totalMileage: 0,
    status: "IN_PROGRESS",
    parcelCount: 12,
    parcelsDelivered: 3,
    estimatedStopCount: 10,
    plannedDistanceMeters: 28900,
    plannedDurationSeconds: 6600,
    createdAt: "2024-03-18T08:50:00Z",
    updatedAt: null,
    cancellationReason: null,
    path: [],
    stops: [],
    assignmentAuditTrail: [],
  },
  {
    id: "20000000-0000-0000-0000-000000000003",
    zoneId: "40000000-0000-0000-0000-000000000002",
    zoneName: "Inner West",
    vehicleId: V2,
    vehiclePlate: "XYZ-5678",
    driverId: "30000000-0000-0000-0000-000000000002",
    driverName: "Jane Doe",
    stagingArea: "A",
    startDate: "2024-03-14T10:00:00Z",
    endDate: "2024-03-14T12:00:00Z",
    startMileage: 120000,
    endMileage: 120050,
    totalMileage: 50,
    status: "COMPLETED",
    parcelCount: 8,
    parcelsDelivered: 8,
    estimatedStopCount: 6,
    plannedDistanceMeters: 14200,
    plannedDurationSeconds: 3300,
    createdAt: "2024-03-14T09:30:00Z",
    updatedAt: null,
    cancellationReason: null,
    path: [],
    stops: [],
    assignmentAuditTrail: [],
  },
];

export function getMockRoutesPaginated(
  vehicleId: string | null,
  page = 1,
  pageSize = 20
): { items: Route[]; totalCount: number; page: number; pageSize: number; totalPages: number } {
  let items = vehicleId
    ? mockRoutes.filter((r) => r.vehicleId === vehicleId)
    : [...mockRoutes];
  items = [...items].sort(
    (a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime()
  );
  const totalCount = items.length;
  const totalPages = Math.ceil(totalCount / pageSize) || 1;
  const start = (page - 1) * pageSize;
  return {
    items: items.slice(start, start + pageSize),
    totalCount,
    page,
    pageSize,
    totalPages,
  };
}

export function getMockRoutesByVehicle(vehicleId: string): Route[] {
  return mockRoutes.filter((r) => r.vehicleId === vehicleId);
}

export function getMockRouteById(id: string): Route | undefined {
  return mockRoutes.find((r) => r.id === id);
}
