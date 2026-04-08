// These are UI-facing models, not raw GraphQL transport types:
// we strip __typename and normalize nested collections to plain arrays for component use.
export interface DepotStorageLayout {
  depotId: string;
  depotName: string;
  availableDeliveryZones: DeliveryZoneOption[];
  storageZones: StorageZone[];
}

export interface DeliveryZoneOption {
  id: string;
  name: string;
}

export interface StorageZone {
  id: string;
  name: string;
  depotId: string;
  storageAisles: StorageAisle[];
}

export interface StorageAisle {
  id: string;
  name: string;
  storageZoneId: string;
  binLocations: BinLocation[];
}

export interface BinLocation {
  id: string;
  name: string;
  isActive: boolean;
  storageAisleId: string;
  deliveryZoneId: string | null;
  deliveryZoneName: string | null;
}

export interface CreateStorageZoneRequest {
  depotId: string;
  name: string;
}

export interface UpdateStorageZoneRequest {
  name: string;
}

export interface CreateStorageAisleRequest {
  storageZoneId: string;
  name: string;
}

export interface UpdateStorageAisleRequest {
  name: string;
}

export interface CreateBinLocationRequest {
  storageAisleId: string;
  name: string;
  isActive: boolean;
  deliveryZoneId?: string | null;
}

export interface UpdateBinLocationRequest {
  name: string;
  isActive?: boolean;
  deliveryZoneId?: string | null;
}
