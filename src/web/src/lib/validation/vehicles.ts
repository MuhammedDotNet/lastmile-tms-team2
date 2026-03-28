import { z } from "zod";


import { guidString } from "@/lib/validation/guid-string";

const vehicleStatusValues = ["AVAILABLE", "IN_USE", "MAINTENANCE", "RETIRED"] as const;
const vehicleTypeValues = ["BIKE", "CAR", "VAN"] as const;

const uuidMsg = "Select a value from the list.";

export const vehicleCreateFormSchema = z.object({
  registrationPlate: z
    .string()
    .trim()
    .min(1, "Registration plate is required.")
    .max(50, "Maximum 50 characters."),
  type: z.enum(vehicleTypeValues),
  parcelCapacity: z
    .number()
    .int("Must be a whole number.")
    .min(1, "Parcel capacity must be at least 1."),
  weightCapacity: z
    .number()
    .finite()
    .positive("Weight capacity must be greater than 0."),
  status: z.enum(vehicleStatusValues).refine((v) => v === "AVAILABLE", {
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
  type: z.enum(vehicleTypeValues),
  parcelCapacity: z
    .number()
    .int("Must be a whole number.")
    .min(1, "Parcel capacity must be at least 1."),
  weightCapacity: z
    .number()
    .finite()
    .positive("Weight capacity must be greater than 0."),
  status: z.enum(vehicleStatusValues),
  depotId: guidString(uuidMsg),
});

export type VehicleCreateFormValues = z.infer<typeof vehicleCreateFormSchema>;
export type VehicleEditFormValues = z.infer<typeof vehicleEditFormSchema>;
