using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class StorageZoneConfiguration : IEntityTypeConfiguration<StorageZone>
{
    public void Configure(EntityTypeBuilder<StorageZone> builder)
    {
        builder.ToTable("StorageZones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne(x => x.Depot)
            .WithMany(x => x.StorageZones)
            .HasForeignKey(x => x.DepotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StorageAisles)
            .WithOne(x => x.StorageZone)
            .HasForeignKey(x => x.StorageZoneId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.DepotId, x.NormalizedName })
            .IsUnique();
    }
}
