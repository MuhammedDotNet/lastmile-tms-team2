using System.Globalization;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.Support;

public static class ParcelImportRowMapper
{
    public static ParcelImportRowMappingResult TryMap(
        Guid shipperAddressId,
        IReadOnlyDictionary<string, string?> values)
    {
        var errors = new List<string>();

        var serviceType = ParseEnum<ServiceType>(
            values,
            "service_type",
            "ECONOMY, STANDARD, EXPRESS, or OVERNIGHT",
            errors);
        var weightUnit = ParseEnum<WeightUnit>(
            values,
            "weight_unit",
            "KG or LB",
            errors);
        var dimensionUnit = ParseEnum<DimensionUnit>(
            values,
            "dimension_unit",
            "CM or IN",
            errors);

        var recipientIsResidential = true;
        var residentialValue = Read(values, "recipient_is_residential");
        if (!string.IsNullOrWhiteSpace(residentialValue) &&
            !bool.TryParse(residentialValue, out recipientIsResidential))
        {
            errors.Add("recipient_is_residential must be true or false.");
        }

        var estimatedDeliveryDate = default(DateTime);
        var estimatedDeliveryDateValue = Read(values, "estimated_delivery_date");
        if (!string.IsNullOrWhiteSpace(estimatedDeliveryDateValue) &&
            !DateTime.TryParseExact(
                estimatedDeliveryDateValue,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out estimatedDeliveryDate))
        {
            errors.Add("estimated_delivery_date must use YYYY-MM-DD format.");
        }

        var weight = ParseDecimal(values, "weight", errors);
        var length = ParseDecimal(values, "length", errors);
        var width = ParseDecimal(values, "width", errors);
        var height = ParseDecimal(values, "height", errors);
        var declaredValue = ParseDecimal(values, "declared_value", errors);

        if (errors.Count > 0)
        {
            return ParcelImportRowMappingResult.Failure(string.Join("; ", errors));
        }

        return ParcelImportRowMappingResult.Success(
            new RegisterParcelDto
            {
                ShipperAddressId = shipperAddressId,
                RecipientAddress = new RegisterParcelRecipientAddressDto
                {
                    Street1 = Read(values, "recipient_street1") ?? string.Empty,
                    Street2 = Read(values, "recipient_street2"),
                    City = Read(values, "recipient_city") ?? string.Empty,
                    State = Read(values, "recipient_state") ?? string.Empty,
                    PostalCode = Read(values, "recipient_postal_code") ?? string.Empty,
                    CountryCode = Read(values, "recipient_country_code") ?? string.Empty,
                    IsResidential = recipientIsResidential,
                    ContactName = Read(values, "recipient_contact_name"),
                    CompanyName = Read(values, "recipient_company_name"),
                    Phone = Read(values, "recipient_phone"),
                    Email = Read(values, "recipient_email"),
                },
                Description = Read(values, "description"),
                ParcelType = Read(values, "parcel_type"),
                ServiceType = serviceType ?? ServiceType.Standard,
                Weight = weight,
                WeightUnit = weightUnit ?? WeightUnit.Kg,
                Length = length,
                Width = width,
                Height = height,
                DimensionUnit = dimensionUnit ?? DimensionUnit.Cm,
                DeclaredValue = declaredValue,
                Currency = Read(values, "currency") ?? string.Empty,
                EstimatedDeliveryDate = estimatedDeliveryDate,
            });
    }

    private static string? Read(IReadOnlyDictionary<string, string?> values, string key)
    {
        if (!values.TryGetValue(key, out var value))
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static decimal ParseDecimal(
        IReadOnlyDictionary<string, string?> values,
        string key,
        ICollection<string> errors)
    {
        var value = Read(values, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0m;
        }

        if (decimal.TryParse(
            value,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var parsedValue))
        {
            return parsedValue;
        }

        errors.Add($"{key} must be a valid number.");
        return 0m;
    }

    private static TEnum? ParseEnum<TEnum>(
        IReadOnlyDictionary<string, string?> values,
        string key,
        string allowedValues,
        ICollection<string> errors)
        where TEnum : struct, Enum
    {
        var value = Read(values, key);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (Enum.TryParse<TEnum>(value, ignoreCase: true, out var parsedValue))
        {
            return parsedValue;
        }

        errors.Add($"{key} must be one of {allowedValues}.");
        return null;
    }
}

public sealed record ParcelImportRowMappingResult(
    bool IsSuccess,
    RegisterParcelDto? Dto,
    string? ErrorMessage)
{
    public static ParcelImportRowMappingResult Success(RegisterParcelDto dto) =>
        new(true, dto, null);

    public static ParcelImportRowMappingResult Failure(string errorMessage) =>
        new(false, null, errorMessage);
}
