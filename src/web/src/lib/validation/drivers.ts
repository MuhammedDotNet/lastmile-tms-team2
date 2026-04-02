import { z } from "zod";
import { guidString } from "@/lib/validation/guid-string";

const driverStatusValues = ["ACTIVE", "INACTIVE", "ON_LEAVE", "SUSPENDED"] as const;
const dayOfWeekValues = [
  "SUNDAY",
  "MONDAY",
  "TUESDAY",
  "WEDNESDAY",
  "THURSDAY",
  "FRIDAY",
  "SATURDAY",
] as const;

const uuidMsg = "Select a value from the list.";

const availabilitySchema = z.object({
  dayOfWeek: z.enum(dayOfWeekValues),
  shiftStart: z.string().nullable().optional(),
  shiftEnd: z.string().nullable().optional(),
  isAvailable: z.boolean(),
});

const driverFormFieldsSchema = z.object({
  firstName: z
    .string()
    .trim()
    .min(1, "First name is required.")
    .max(100, "Maximum 100 characters."),
  lastName: z
    .string()
    .trim()
    .min(1, "Last name is required.")
    .max(100, "Maximum 100 characters."),
  phone: z.string().nullable().optional(),
  email: z.string().nullable().optional().refine(
    (v) => !v || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v),
    { message: "Invalid email address." },
  ),
  licenseNumber: z
    .string()
    .trim()
    .min(1, "License number is required.")
    .max(50, "Maximum 50 characters."),
  licenseExpiryDate: z.string().nullable().optional(),
  zoneId: guidString(uuidMsg),
  depotId: guidString(uuidMsg),
  status: z.enum(driverStatusValues),
  userId: guidString(uuidMsg),
  availabilitySchedule: z.array(availabilitySchema),
});

export const driverCreateFormSchema = driverFormFieldsSchema.superRefine(
  (data, ctx) => {
    const days = data.availabilitySchedule.map((a) => a.dayOfWeek);
    if (new Set(days).size !== days.length) {
      ctx.addIssue({
        code: "custom",
        message: "Each day of the week can appear only once.",
        path: ["availabilitySchedule"],
      });
    }
  },
);

export const driverEditFormSchema = driverCreateFormSchema;

export type DriverCreateFormValues = z.infer<typeof driverCreateFormSchema>;
export type DriverEditFormValues = z.infer<typeof driverEditFormSchema>;
