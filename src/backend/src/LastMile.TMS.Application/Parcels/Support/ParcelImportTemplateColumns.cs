namespace LastMile.TMS.Application.Parcels.Support;

public static class ParcelImportTemplateColumns
{
    public static readonly string[] All =
    [
        "recipient_street1",
        "recipient_street2",
        "recipient_city",
        "recipient_state",
        "recipient_postal_code",
        "recipient_country_code",
        "recipient_is_residential",
        "recipient_contact_name",
        "recipient_company_name",
        "recipient_phone",
        "recipient_email",
        "description",
        "parcel_type",
        "service_type",
        "weight",
        "weight_unit",
        "length",
        "width",
        "height",
        "dimension_unit",
        "declared_value",
        "currency",
        "estimated_delivery_date",
    ];

    public static bool HasCanonicalHeaders(IReadOnlyList<string> headers)
    {
        if (headers.Count != All.Length)
        {
            return false;
        }

        for (var i = 0; i < All.Length; i++)
        {
            if (!string.Equals(headers[i], All[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
