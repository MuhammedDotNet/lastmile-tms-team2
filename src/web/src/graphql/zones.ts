export {
  GetZonesDocument as ZONES_LIST,
  GetZoneDocument as ZONE_BY_ID,
  CreateZoneDocument as CREATE_ZONE,
  UpdateZoneDocument as UPDATE_ZONE,
  DeleteZoneDocument as DELETE_ZONE,
} from "./generated";
export type {
  GetZonesQuery,
  GetZonesQueryVariables,
  GetZoneQuery,
  GetZoneQueryVariables,
  CreateZoneMutation,
  CreateZoneMutationVariables,
  UpdateZoneMutation,
  UpdateZoneMutationVariables,
  DeleteZoneMutation,
  DeleteZoneMutationVariables,
} from "./generated";
