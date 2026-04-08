import type {
  BinLocation,
  StorageAisle,
  StorageZone,
} from "@/types/bin-locations";

export type CreateState =
  | { kind: "zone" }
  | { kind: "aisle"; storageZoneId: string }
  | { kind: "bin"; storageAisleId: string }
  | null;

export type EditState =
  | { kind: "zone"; zone: StorageZone }
  | { kind: "aisle"; aisle: StorageAisle }
  | { kind: "bin"; bin: BinLocation }
  | null;

export type DeleteState =
  | { kind: "zone"; id: string; name: string }
  | { kind: "aisle"; id: string; name: string }
  | { kind: "bin"; id: string; name: string }
  | null;
