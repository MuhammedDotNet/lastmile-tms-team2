using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class BinLocationConfiguration : IEntityTypeConfiguration<BinLocation>
{
    public void Configure(EntityTypeBuilder<BinLocation> builder)
    {
        builder.ToTable("BinLocations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.StorageAisle)
            .WithMany(x => x.BinLocations)
            .HasForeignKey(x => x.StorageAisleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.DeliveryZone)
            .WithMany(x => x.AssignedBinLocations)
            .HasForeignKey(x => x.DeliveryZoneId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.StorageAisleId, x.NormalizedName })
            .IsUnique();

        builder.HasIndex(x => x.DeliveryZoneId)
            .IsUnique()
            .HasFilter("\"DeliveryZoneId\" IS NOT NULL AND \"IsActive\" = TRUE");
    }
}
