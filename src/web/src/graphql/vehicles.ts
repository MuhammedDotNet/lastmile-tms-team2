export {
  GetVehiclesDocument as PAGINATED_VEHICLES,
  GetVehicleDocument as VEHICLE_BY_ID,
  CreateVehicleDocument as CREATE_VEHICLE,
  UpdateVehicleDocument as UPDATE_VEHICLE,
  DeleteVehicleDocument as DELETE_VEHICLE,
} from "./generated";
export type {
  GetVehiclesQuery,
  GetVehiclesQueryVariables,
  GetVehicleQuery,
  GetVehicleQueryVariables,
  CreateVehicleMutation,
  CreateVehicleMutationVariables,
  UpdateVehicleMutation,
  UpdateVehicleMutationVariables,
  DeleteVehicleMutation,
  DeleteVehicleMutationVariables,
} from "./generated";
