/** GraphQL documents for the dispatcher app (Hot Chocolate: `Get*` C# methods → fields without `get`, e.g. `GetVehicles` → `vehicles`). */

export const VEHICLE_FIELDS = `
  id
  registrationPlate
  type
  parcelCapacity
  weightCapacity
  status
  depotId
  depotName
  totalRoutes
  routesCompleted
  totalMileage
  createdAt
  lastModifiedAt
`;

export const ROUTE_FIELDS = `
  id
  vehicleId
  vehiclePlate
  driverId
  driverName
  startDate
  endDate
  startMileage
  endMileage
  totalMileage
  status
  parcelCount
  parcelsDelivered
  createdAt
`;

export const PAGINATED_VEHICLES = `
  query GetVehicles($page: Int, $pageSize: Int, $status: VehicleStatus, $depotId: UUID) {
    vehicles(page: $page, pageSize: $pageSize, status: $status, depotId: $depotId) {
      items { ${VEHICLE_FIELDS} }
      totalCount
      page
      pageSize
      totalPages
    }
  }
`;

export const VEHICLE_BY_ID = `
  query GetVehicle($id: UUID!) {
    vehicle(id: $id) {
      ${VEHICLE_FIELDS}
    }
  }
`;

export const PAGINATED_ROUTES = `
  query GetRoutes($vehicleId: UUID, $status: RouteStatus, $page: Int, $pageSize: Int) {
    routes(vehicleId: $vehicleId, status: $status, page: $page, pageSize: $pageSize) {
      items { ${ROUTE_FIELDS} }
      totalCount
      page
      pageSize
      totalPages
    }
  }
`;

export const ROUTE_BY_ID = `
  query GetRoute($id: UUID!) {
    route(id: $id) {
      ${ROUTE_FIELDS}
    }
  }
`;

export const VEHICLE_ROUTE_HISTORY = `
  query GetVehicleHistory($vehicleId: UUID!, $status: RouteStatus, $page: Int, $pageSize: Int) {
    vehicleHistory(vehicleId: $vehicleId, status: $status, page: $page, pageSize: $pageSize) {
      items { ${ROUTE_FIELDS} }
      totalCount
      page
      pageSize
      totalPages
    }
  }
`;

export const PARCELS_FOR_ROUTE = `
  query GetParcelsForRouteCreation {
    parcelsForRouteCreation {
      id
      trackingNumber
      weight
      weightUnit
    }
  }
`;

export const DEPOTS_LIST = `
  query GetDepots {
    depots {
      id
      name
    }
  }
`;

export const DRIVERS_LIST = `
  query GetDrivers($depotId: UUID) {
    drivers(depotId: $depotId) {
      id
      displayName
    }
  }
`;

export const MUTATION_CREATE_VEHICLE = `
  mutation CreateVehicle($input: CreateVehicleDtoInput!) {
    createVehicle(input: $input) {
      ${VEHICLE_FIELDS}
    }
  }
`;

export const MUTATION_UPDATE_VEHICLE = `
  mutation UpdateVehicle($id: UUID!, $input: UpdateVehicleDtoInput!) {
    updateVehicle(id: $id, input: $input) {
      ${VEHICLE_FIELDS}
    }
  }
`;

export const MUTATION_DELETE_VEHICLE = `
  mutation DeleteVehicle($id: UUID!) {
    deleteVehicle(id: $id)
  }
`;

export const MUTATION_CREATE_ROUTE = `
  mutation CreateRoute($input: CreateRouteDtoInput!) {
    createRoute(input: $input) {
      ${ROUTE_FIELDS}
    }
  }
`;
