export {
  GetDepotsDocument as DEPOTS_LIST,
  GetDepotDocument as DEPOT_BY_ID,
  CreateDepotDocument as CREATE_DEPOT,
  UpdateDepotDocument as UPDATE_DEPOT,
  DeleteDepotDocument as DELETE_DEPOT,
} from "./generated";
export type {
  GetDepotsQuery,
  GetDepotsQueryVariables,
  GetDepotQuery,
  GetDepotQueryVariables,
  CreateDepotMutation,
  CreateDepotMutationVariables,
  UpdateDepotMutation,
  UpdateDepotMutationVariables,
  DeleteDepotMutation,
  DeleteDepotMutationVariables,
} from "./generated";
