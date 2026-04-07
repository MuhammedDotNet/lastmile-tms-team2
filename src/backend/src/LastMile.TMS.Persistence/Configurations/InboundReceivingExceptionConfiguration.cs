using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class InboundReceivingExceptionConfiguration : IEntityTypeConfiguration<InboundReceivingException>
{
    public void Configure(EntityTypeBuilder<InboundReceivingException> builder)
    {
        builder.ToTable("InboundReceivingExceptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TrackingNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Barcode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ExceptionType)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(x => new { x.SessionId, x.ExceptionType, x.Barcode })
            .IsUnique();

        builder.HasOne(x => x.Parcel)
            .WithMany()
            .HasForeignKey(x => x.ParcelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ManifestLine)
            .WithMany(x => x.Exceptions)
            .HasForeignKey(x => x.ManifestLineId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
