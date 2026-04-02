import {
  DRIVERS_LIST,
  DRIVER_DETAIL,
  CREATE_DRIVER,
  UPDATE_DRIVER,
  DELETE_DRIVER,
} from "@/graphql/drivers";
import type {
  GetDriversQuery,
  GetDriverQuery,
  CreateDriverMutation,
  UpdateDriverMutation,
} from "@/graphql/drivers";
import type { DriverFilterInput } from "@/graphql/generated";
import { graphqlRequest } from "@/lib/network/graphql-client";
import { toGraphQLDateTimeFromDateInput } from "@/lib/datetime/graphql-datetime";
import {
  timeSpanScalarToHms,
  toGraphQLTimeSpanFromHms,
} from "@/lib/time/graphql-timespan";
import { sortByDayOfWeek } from "@/lib/labels/drivers";
import { mockDrivers } from "@/mocks/drivers.mock";
import type {
  Driver,
  CreateDriverRequest,
  UpdateDriverRequest,
  CreateDriverAvailabilityRequest,
  UpdateDriverAvailabilityRequest,
} from "@/types/drivers";

function mapCreateAvailabilityForGraphQL(
  rows: CreateDriverAvailabilityRequest[],
) {
  return rows.map((a) => ({
    dayOfWeek: a.dayOfWeek,
    isAvailable: a.isAvailable,
    shiftStart:
      a.isAvailable && a.shiftStart?.trim()
        ? toGraphQLTimeSpanFromHms(a.shiftStart)
        : null,
    shiftEnd:
      a.isAvailable && a.shiftEnd?.trim()
        ? toGraphQLTimeSpanFromHms(a.shiftEnd)
        : null,
  }));
}

function mapUpdateAvailabilityForGraphQL(
  rows: UpdateDriverAvailabilityRequest[],
) {
  return rows.map((a) => ({
    id: a.id,
    dayOfWeek: a.dayOfWeek,
    isAvailable: a.isAvailable,
    shiftStart:
      a.isAvailable && a.shiftStart?.trim()
        ? toGraphQLTimeSpanFromHms(a.shiftStart)
        : null,
    shiftEnd:
      a.isAvailable && a.shiftEnd?.trim()
        ? toGraphQLTimeSpanFromHms(a.shiftEnd)
        : null,
  }));
}

const USE_MOCK = process.env.NEXT_PUBLIC_USE_MOCK_DATA === "true";

export const driversService = {
  getAll: async (where?: DriverFilterInput): Promise<Driver[]> => {
    if (USE_MOCK) {
      return mockDrivers.map((d) => ({
        id: d.id,
        displayName: d.name,
        firstName: d.name.split(" ")[0],
        lastName: d.name.split(" ")[1] || "",
        phone: null,
        email: null,
        licenseNumber: "LIC-00001",
        licenseExpiryDate: null,
        photoUrl: null,
        zoneId: "00000000-0000-0000-0000-000000000001",
        depotId: "00000000-0000-0000-0000-000000000001",
        status: "ACTIVE" as const,
        userId: "00000000-0000-0000-0000-000000000001",
        zoneName: "Zone 1",
        depotName: "Depot 1",
        userName: d.name.toLowerCase().replace(" ", "."),
        availabilitySchedule: [],
        createdAt: new Date().toISOString(),
        updatedAt: null,
      }));
    }

    const variables: Record<string, unknown> = {};
    if (where !== undefined) {
      variables.where = where;
    }

    const data = await graphqlRequest<GetDriversQuery>(
      DRIVERS_LIST,
      Object.keys(variables).length ? variables : undefined
    );
    return data.drivers.map((d) => ({
      id: d.id,
      displayName: d.displayName,
      firstName: d.firstName,
      lastName: d.lastName,
      phone: d.phone,
      email: d.email,
      licenseNumber: d.licenseNumber,
      licenseExpiryDate: d.licenseExpiryDate,
      photoUrl: d.photoUrl,
      zoneId: d.zoneId,
      depotId: d.depotId,
      status: d.status,
      userId: d.userId,
      zoneName: d.zoneName ?? null,
      depotName: d.depotName ?? null,
      userName: d.userName ?? null,
      availabilitySchedule: [],
      createdAt: d.createdAt,
      updatedAt: d.updatedAt ?? null,
    }));
  },

  getById: async (id: string): Promise<Driver> => {
    if (USE_MOCK) {
      const mockDriver = mockDrivers.find((d) => d.id === id);
      if (!mockDriver) throw new Error("Driver not found");
      return {
        id: mockDriver.id,
        displayName: mockDriver.name,
        firstName: mockDriver.name.split(" ")[0],
        lastName: mockDriver.name.split(" ")[1] || "",
        phone: "+1-555-0100",
        email: `${mockDriver.name.toLowerCase().replace(" ", ".")}@example.com`,
        licenseNumber: "LIC-00001",
        licenseExpiryDate: "2027-12-31T00:00:00.000Z",
        photoUrl: null,
        zoneId: "00000000-0000-0000-0000-000000000001",
        depotId: "00000000-0000-0000-0000-000000000001",
        status: "ACTIVE" as const,
        userId: "00000000-0000-0000-0000-000000000001",
        zoneName: "Zone 1",
        depotName: "Depot 1",
        userName: mockDriver.name.toLowerCase().replace(" ", "."),
        availabilitySchedule: [
          {
            id: "avail-1",
            dayOfWeek: "MONDAY" as const,
            shiftStart: "08:00:00",
            shiftEnd: "17:00:00",
            isAvailable: true,
          },
          {
            id: "avail-2",
            dayOfWeek: "TUESDAY" as const,
            shiftStart: "08:00:00",
            shiftEnd: "17:00:00",
            isAvailable: true,
          },
          {
            id: "avail-3",
            dayOfWeek: "WEDNESDAY" as const,
            shiftStart: "08:00:00",
            shiftEnd: "17:00:00",
            isAvailable: true,
          },
          {
            id: "avail-4",
            dayOfWeek: "THURSDAY" as const,
            shiftStart: "08:00:00",
            shiftEnd: "17:00:00",
            isAvailable: true,
          },
          {
            id: "avail-5",
            dayOfWeek: "FRIDAY" as const,
            shiftStart: "08:00:00",
            shiftEnd: "17:00:00",
            isAvailable: true,
          },
          {
            id: "avail-6",
            dayOfWeek: "SATURDAY" as const,
            shiftStart: null,
            shiftEnd: null,
            isAvailable: false,
          },
          {
            id: "avail-7",
            dayOfWeek: "SUNDAY" as const,
            shiftStart: null,
            shiftEnd: null,
            isAvailable: false,
          },
        ],
        createdAt: new Date().toISOString(),
        updatedAt: null,
      };
    }

    const data = await graphqlRequest<GetDriverQuery>(DRIVER_DETAIL, { id });
    if (!data.driver) throw new Error("Driver not found");
    const d = data.driver;
    return {
      id: d.id,
      displayName: d.displayName,
      firstName: d.firstName,
      lastName: d.lastName,
      phone: d.phone,
      email: d.email,
      licenseNumber: d.licenseNumber,
      licenseExpiryDate: d.licenseExpiryDate,
      photoUrl: d.photoUrl,
      zoneId: d.zoneId,
      depotId: d.depotId,
      status: d.status,
      userId: d.userId,
      zoneName: d.zoneName ?? null,
      depotName: d.depotName ?? null,
      userName: d.userName ?? null,
      availabilitySchedule: sortByDayOfWeek(
        (d.availabilitySchedule ?? []).map((a) => ({
          id: a.id,
          dayOfWeek: a.dayOfWeek,
          shiftStart: timeSpanScalarToHms(a.shiftStart),
          shiftEnd: timeSpanScalarToHms(a.shiftEnd),
          isAvailable: a.isAvailable,
        })),
      ),
      createdAt: d.createdAt,
      updatedAt: d.updatedAt ?? null,
    };
  },

  create: async (data: CreateDriverRequest): Promise<Driver> => {
    if (USE_MOCK) {
      const newDriver: Driver = {
        id: `mock-${Date.now()}`,
        displayName: `${data.firstName} ${data.lastName}`,
        firstName: data.firstName,
        lastName: data.lastName,
        phone: data.phone ?? null,
        email: data.email ?? null,
        licenseNumber: data.licenseNumber,
        licenseExpiryDate: data.licenseExpiryDate ?? null,
        photoUrl: data.photoUrl ?? null,
        zoneId: data.zoneId,
        depotId: data.depotId,
        status: data.status,
        userId: data.userId,
        zoneName: "Zone 1",
        depotName: "Depot 1",
        userName: null,
        availabilitySchedule: sortByDayOfWeek(
          data.availabilitySchedule.map((a, i) => ({
            id: `mock-avail-${i}`,
            dayOfWeek: a.dayOfWeek,
            shiftStart: a.shiftStart ?? null,
            shiftEnd: a.shiftEnd ?? null,
            isAvailable: a.isAvailable,
          })),
        ),
        createdAt: new Date().toISOString(),
        updatedAt: null,
      };
      return Promise.resolve(newDriver);
    }

    const res = await graphqlRequest<CreateDriverMutation>(CREATE_DRIVER, {
      input: {
        firstName: data.firstName,
        lastName: data.lastName,
        phone: data.phone,
        email: data.email,
        licenseNumber: data.licenseNumber,
        licenseExpiryDate: toGraphQLDateTimeFromDateInput(
          data.licenseExpiryDate ?? null,
        ),
        photoUrl: data.photoUrl,
        zoneId: data.zoneId,
        depotId: data.depotId,
        status: data.status,
        userId: data.userId,
        availabilitySchedule: mapCreateAvailabilityForGraphQL(
          data.availabilitySchedule,
        ),
      },
    });
    const d = res.createDriver;
    return {
      id: d.id,
      displayName: d.displayName,
      firstName: d.firstName,
      lastName: d.lastName,
      phone: d.phone,
      email: d.email,
      licenseNumber: d.licenseNumber,
      licenseExpiryDate: d.licenseExpiryDate,
      photoUrl: d.photoUrl,
      zoneId: d.zoneId,
      depotId: d.depotId,
      status: d.status,
      userId: d.userId,
      zoneName: d.zoneName ?? null,
      depotName: d.depotName ?? null,
      userName: d.userName ?? null,
      availabilitySchedule: sortByDayOfWeek(
        (d.availabilitySchedule ?? []).map((a) => ({
          id: a.id,
          dayOfWeek: a.dayOfWeek,
          shiftStart: timeSpanScalarToHms(a.shiftStart),
          shiftEnd: timeSpanScalarToHms(a.shiftEnd),
          isAvailable: a.isAvailable,
        })),
      ),
      createdAt: d.createdAt,
      updatedAt: d.updatedAt ?? null,
    };
  },

  update: async (id: string, data: UpdateDriverRequest): Promise<Driver> => {
    if (USE_MOCK) {
      const updated: Driver = {
        id,
        displayName: `${data.firstName} ${data.lastName}`,
        firstName: data.firstName,
        lastName: data.lastName,
        phone: data.phone ?? null,
        email: data.email ?? null,
        licenseNumber: data.licenseNumber,
        licenseExpiryDate: data.licenseExpiryDate ?? null,
        photoUrl: data.photoUrl ?? null,
        zoneId: data.zoneId,
        depotId: data.depotId,
        status: data.status,
        userId: data.userId,
        zoneName: "Zone 1",
        depotName: "Depot 1",
        userName: null,
        availabilitySchedule: sortByDayOfWeek(
          data.availabilitySchedule.map((a, i) => ({
            id: a.id ?? `mock-avail-${i}`,
            dayOfWeek: a.dayOfWeek,
            shiftStart: a.shiftStart ?? null,
            shiftEnd: a.shiftEnd ?? null,
            isAvailable: a.isAvailable,
          })),
        ),
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
      return Promise.resolve(updated);
    }

    const res = await graphqlRequest<UpdateDriverMutation>(UPDATE_DRIVER, {
      id,
      input: {
        firstName: data.firstName,
        lastName: data.lastName,
        phone: data.phone,
        email: data.email,
        licenseNumber: data.licenseNumber,
        licenseExpiryDate: toGraphQLDateTimeFromDateInput(
          data.licenseExpiryDate ?? null,
        ),
        photoUrl: data.photoUrl,
        zoneId: data.zoneId,
        depotId: data.depotId,
        status: data.status,
        userId: data.userId,
        availabilitySchedule: mapUpdateAvailabilityForGraphQL(
          data.availabilitySchedule,
        ),
      },
    });
    if (!res.updateDriver) throw new Error("Driver not found");
    const d = res.updateDriver;
    return {
      id: d.id,
      displayName: d.displayName,
      firstName: d.firstName,
      lastName: d.lastName,
      phone: d.phone,
      email: d.email,
      licenseNumber: d.licenseNumber,
      licenseExpiryDate: d.licenseExpiryDate,
      photoUrl: d.photoUrl,
      zoneId: d.zoneId,
      depotId: d.depotId,
      status: d.status,
      userId: d.userId,
      zoneName: d.zoneName ?? null,
      depotName: d.depotName ?? null,
      userName: d.userName ?? null,
      availabilitySchedule: sortByDayOfWeek(
        (d.availabilitySchedule ?? []).map((a) => ({
          id: a.id,
          dayOfWeek: a.dayOfWeek,
          shiftStart: timeSpanScalarToHms(a.shiftStart),
          shiftEnd: timeSpanScalarToHms(a.shiftEnd),
          isAvailable: a.isAvailable,
        })),
      ),
      createdAt: d.createdAt,
      updatedAt: d.updatedAt ?? null,
    };
  },

  delete: async (id: string): Promise<boolean> => {
    if (USE_MOCK) {
      const exists = mockDrivers.find((d) => d.id === id);
      if (!exists) throw new Error("Driver not found");
      return Promise.resolve(true);
    }

    const res = await graphqlRequest<{ deleteDriver: boolean }>(
      DELETE_DRIVER,
      { id }
    );
    return res.deleteDriver;
  },
};
