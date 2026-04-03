import {
  ParcelDimensionUnit,
  ParcelServiceType,
  ParcelWeightUnit,
  type GraphQLDimensionUnit,
  type GraphQLServiceType,
  type ParcelDetail,
  type ParcelFormData,
} from "@/types/parcels";

function toLocalWeightUnit(value: string): number {
  return value.toUpperCase() === "LB" ? ParcelWeightUnit.Lb : ParcelWeightUnit.Kg;
}

function toLocalDimensionUnit(value: string): GraphQLDimensionUnit {
  return value.toUpperCase() === "IN" ? ParcelDimensionUnit.In : ParcelDimensionUnit.Cm;
}

function toLocalServiceType(value: string): GraphQLServiceType {
  if (
    Object.values(ParcelServiceType).includes(value as GraphQLServiceType)
  ) {
    return value as GraphQLServiceType;
  }

  return ParcelServiceType.Standard;
}

function toDateInput(value: string): string {
  return value.slice(0, 10);
}

export function createEmptyParcelFormData(): ParcelFormData {
  return {
    shipperAddressId: "",
    recipientStreet1: "",
    recipientStreet2: "",
    recipientCity: "",
    recipientState: "",
    recipientPostalCode: "",
    recipientCountryCode: "US",
    recipientIsResidential: true,
    recipientContactName: "",
    recipientCompanyName: "",
    recipientPhone: "",
    recipientEmail: "",
    description: "",
    parcelType: "",
    serviceType: ParcelServiceType.Standard,
    weight: 1,
    weightUnit: ParcelWeightUnit.Kg,
    length: 10,
    width: 10,
    height: 10,
    dimensionUnit: ParcelDimensionUnit.Cm,
    declaredValue: 0,
    currency: "USD",
    estimatedDeliveryDate: "",
  };
}

export function parcelDetailToFormData(parcel: ParcelDetail): ParcelFormData {
  return {
    shipperAddressId: parcel.shipperAddressId,
    recipientStreet1: parcel.recipientAddress.street1,
    recipientStreet2: parcel.recipientAddress.street2 ?? "",
    recipientCity: parcel.recipientAddress.city,
    recipientState: parcel.recipientAddress.state,
    recipientPostalCode: parcel.recipientAddress.postalCode,
    recipientCountryCode: parcel.recipientAddress.countryCode,
    recipientIsResidential: parcel.recipientAddress.isResidential,
    recipientContactName: parcel.recipientAddress.contactName ?? "",
    recipientCompanyName: parcel.recipientAddress.companyName ?? "",
    recipientPhone: parcel.recipientAddress.phone ?? "",
    recipientEmail: parcel.recipientAddress.email ?? "",
    description: parcel.description ?? "",
    parcelType: parcel.parcelType ?? "",
    serviceType: toLocalServiceType(parcel.serviceType),
    weight: parcel.weight,
    weightUnit: toLocalWeightUnit(parcel.weightUnit),
    length: parcel.length,
    width: parcel.width,
    height: parcel.height,
    dimensionUnit: toLocalDimensionUnit(parcel.dimensionUnit),
    declaredValue: parcel.declaredValue,
    currency: parcel.currency,
    estimatedDeliveryDate: toDateInput(parcel.estimatedDeliveryDate),
  };
}
