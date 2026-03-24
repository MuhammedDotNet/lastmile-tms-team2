import { z } from "zod";

import { VehicleStatus, VehicleType } from "@/types/vehicles";
import { guidString } from "@/lib/validation/guid-string";

const uuidMsg = "Select a value from the list.";

export const vehicleCreateFormSchema = z.object({
  registrationPlate: z
    .string()
    .trim()
    .min(1, "Registration plate is required.")
    .max(50, "Maximum 50 characters."),
  type: z.nativeEnum(VehicleType),
  parcelCapacity: z
    .number()
    .int("Must be a whole number.")
    .min(1, "Parcel capacity must be at least 1."),
  weightCapacity: z
    .number()
    .finite()
    .positive("Weight capacity must be greater than 0."),
  status: z.nativeEnum(VehicleStatus).refine((v) => v === VehicleStatus.Available, {
    message: "New vehicles must start as Available.",
  }),
  depotId: guidString(uuidMsg),
});

export const vehicleEditFormSchema = z.object({
  registrationPlate: z
    .string()
    .trim()
    .min(1, "Registration plate is required.")
    .max(50, "Maximum 50 characters."),
  type: z.nativeEnum(VehicleType),
  parcelCapacity: z
    .number()
    .int("Must be a whole number.")
    .min(1, "Parcel capacity must be at least 1."),
  weightCapacity: z
    .number()
    .finite()
    .positive("Weight capacity must be greater than 0."),
  status: z.nativeEnum(VehicleStatus),
  depotId: guidString(uuidMsg),
});

export type VehicleCreateFormValues = z.infer<typeof vehicleCreateFormSchema>;
export type VehicleEditFormValues = z.infer<typeof vehicleEditFormSchema>;
