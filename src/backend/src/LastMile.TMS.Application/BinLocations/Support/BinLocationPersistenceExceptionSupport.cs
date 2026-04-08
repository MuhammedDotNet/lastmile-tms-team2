using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.BinLocations.Support;

internal static class BinLocationPersistenceExceptionSupport
{
    public const string StorageZoneNameIndex = "IX_StorageZones_DepotId_NormalizedName";
    public const string StorageAisleNameIndex = "IX_StorageAisles_StorageZoneId_NormalizedName";
    public const string BinLocationNameIndex = "IX_BinLocations_StorageAisleId_NormalizedName";
    public const string BinLocationDeliveryZoneIndex = "IX_BinLocations_DeliveryZoneId";

    public static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var text = GetExceptionText(ex);

        return text.Contains("unique", StringComparison.OrdinalIgnoreCase)
            || text.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            || text.Contains("23505", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsUniqueConstraintViolation(DbUpdateException ex, string constraintName)
    {
        return IsUniqueConstraintViolation(ex)
            && GetExceptionText(ex).Contains(constraintName, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetExceptionText(DbUpdateException ex)
    {
        return string.Join(
            " ",
            new[] { ex.Message, ex.InnerException?.Message }
                .Where(part => !string.IsNullOrWhiteSpace(part)));
    }
}
