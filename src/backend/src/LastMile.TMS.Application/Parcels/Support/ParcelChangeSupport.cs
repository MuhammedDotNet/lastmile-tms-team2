using System.Globalization;
using System.Text.RegularExpressions;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.Support;

internal static partial class ParcelChangeSupport
{
    public static bool HasGeocodedAddressChanges(Parcel parcel, UpdateParcelDto dto)
    {
        var current = parcel.RecipientAddress;
        var next = dto.RecipientAddress;

        return NormalizeRequired(current.Street1) != NormalizeRequired(next.Street1)
            || NormalizeOptional(current.Street2) != NormalizeOptional(next.Street2)
            || NormalizeRequired(current.City) != NormalizeRequired(next.City)
            || NormalizeRequired(current.State) != NormalizeRequired(next.State)
            || NormalizeRequired(current.PostalCode) != NormalizeRequired(next.PostalCode)
            || NormalizeCountryCode(current.CountryCode) != NormalizeCountryCode(next.CountryCode);
    }

    public static string BuildAddressString(RegisterParcelRecipientAddressDto address)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(address.Street1)) parts.Add(address.Street1.Trim());
        if (!string.IsNullOrWhiteSpace(address.Street2)) parts.Add(address.Street2.Trim());
        if (!string.IsNullOrWhiteSpace(address.City)) parts.Add(address.City.Trim());
        if (!string.IsNullOrWhiteSpace(address.State)) parts.Add(address.State.Trim());
        if (!string.IsNullOrWhiteSpace(address.PostalCode)) parts.Add(address.PostalCode.Trim());
        if (!string.IsNullOrWhiteSpace(address.CountryCode)) parts.Add(address.CountryCode.Trim());
        return string.Join(", ", parts);
    }

    public static string NormalizeRequired(string value) => value.Trim();

    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string NormalizeCountryCode(string value) => value.Trim().ToUpperInvariant();

    public static string NormalizeCurrency(string value) => value.Trim().ToUpperInvariant();

    public static string FormatEnum(Enum value) => SplitCamelCaseRegex().Replace(value.ToString(), " $1").Trim();

    public static string FormatBool(bool value) => value ? "Yes" : "No";

    public static string FormatDecimal(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    public static string FormatDate(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    public static string? FormatNullable(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex SplitCamelCaseRegex();
}
