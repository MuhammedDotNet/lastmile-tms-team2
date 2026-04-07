using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class InboundManifestLineConfiguration : IEntityTypeConfiguration<InboundManifestLine>
{
    public void Configure(EntityTypeBuilder<InboundManifestLine> builder)
    {
        builder.ToTable("InboundManifestLines");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ManifestId, x.ParcelId })
            .IsUnique();

        builder.HasOne(x => x.Parcel)
            .WithMany()
            .HasForeignKey(x => x.ParcelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
