/** Matches backend `WeightUnit`: Lb = 0, Kg = 1 */
export const ParcelWeightUnit = {
  Lb: 0,
  Kg: 1,
} as const;

export interface ParcelOption {
  id: string;
  trackingNumber: string;
  weight: number;
  weightUnit: number;
}
