using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class StorageAisleConfiguration : IEntityTypeConfiguration<StorageAisle>
{
    public void Configure(EntityTypeBuilder<StorageAisle> builder)
    {
        builder.ToTable("StorageAisles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.StorageZone)
            .WithMany(x => x.StorageAisles)
            .HasForeignKey(x => x.StorageZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BinLocations)
            .WithOne(x => x.StorageAisle)
            .HasForeignKey(x => x.StorageAisleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.StorageZoneId, x.NormalizedName })
            .IsUnique();
    }
}
