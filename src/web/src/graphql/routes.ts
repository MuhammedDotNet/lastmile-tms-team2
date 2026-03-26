export {
  GetRoutesDocument as PAGINATED_ROUTES,
  GetRouteDocument as ROUTE_BY_ID,
  GetVehicleHistoryDocument as VEHICLE_ROUTE_HISTORY,
  CreateRouteDocument as CREATE_ROUTE,
} from "./generated";
export type {
  GetRoutesQuery,
  GetRoutesQueryVariables,
  GetRouteQuery,
  GetRouteQueryVariables,
  GetVehicleHistoryQuery,
  GetVehicleHistoryQueryVariables,
  CreateRouteMutation,
  CreateRouteMutationVariables,
} from "./generated";
