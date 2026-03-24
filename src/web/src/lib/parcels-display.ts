import { ParcelWeightUnit } from "@/types/parcels";

export function parcelWeightKg(weight: number, weightUnit: number): number {
  return weightUnit === ParcelWeightUnit.Lb ? weight * 0.453592 : weight;
}

export function formatParcelWeightUnitLabel(weightUnit: number): string {
  return weightUnit === ParcelWeightUnit.Lb ? "Lb" : "Kg";
}
