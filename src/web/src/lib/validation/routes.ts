import { z } from "zod";

const selectMsg = "Select a value from the list.";

export const routeCreateFormSchema = z.object({
  vehicleId: z.string().min(1, selectMsg),
  driverId: z.string().min(1, selectMsg),
  stagingArea: z.enum(["A", "B"]),
  startDate: z
    .string()
    .min(1, "Choose a start date and time.")
    .refine((s) => !Number.isNaN(Date.parse(s)), "Invalid date or time."),
  startMileage: z
    .number()
    .int("Mileage must be a whole number.")
    .min(0, "Mileage cannot be negative."),
  parcelIds: z.array(z.string().min(1, "Invalid parcel id.")),
});

export type RouteCreateFormValues = z.infer<typeof routeCreateFormSchema>;
