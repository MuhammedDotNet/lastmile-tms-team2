using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class InboundManifestConfiguration : IEntityTypeConfiguration<InboundManifest>
{
    public void Configure(EntityTypeBuilder<InboundManifest> builder)
    {
        builder.ToTable("InboundManifests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ManifestNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TruckIdentifier)
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(x => x.ManifestNumber)
            .IsUnique();

        builder.HasIndex(x => new { x.DepotId, x.Status });

        builder.HasOne(x => x.Depot)
            .WithMany()
            .HasForeignKey(x => x.DepotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.Manifest)
            .HasForeignKey(x => x.ManifestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Sessions)
            .WithOne(x => x.Manifest)
            .HasForeignKey(x => x.ManifestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
