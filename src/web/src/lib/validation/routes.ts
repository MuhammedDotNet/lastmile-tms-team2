import { z } from "zod";

const selectMsg = "Select a value from the list.";
const routeAssignmentModeValues = ["MANUAL_PARCELS", "AUTO_BY_ZONE"] as const;
const routeStopModeValues = ["AUTO", "MANUAL"] as const;

export const routeCreateFormSchema = z.object({
  zoneId: z.string().min(1, selectMsg),
  vehicleId: z.string().min(1, selectMsg),
  driverId: z.string().min(1, selectMsg),
  stagingArea: z.enum(["A", "B"]),
  startDate: z
    .string()
    .min(1, "Choose a start date and time.")
    .refine((value) => !Number.isNaN(Date.parse(value)), "Invalid date or time."),
  startMileage: z
    .number()
    .int("Mileage must be a whole number.")
    .min(0, "Mileage cannot be negative."),
  assignmentMode: z.enum(routeAssignmentModeValues),
  stopMode: z.enum(routeStopModeValues),
  parcelIds: z.array(z.string().min(1, "Invalid parcel id.")),
  stops: z.array(
    z.object({
      sequence: z.number().int().min(1, "Invalid stop sequence."),
      parcelIds: z.array(z.string().min(1, "Invalid parcel id.")),
    }),
  ),
});

export type RouteCreateFormValues = z.infer<typeof routeCreateFormSchema>;
