using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class TrackingEventConfiguration : IEntityTypeConfiguration<TrackingEvent>
{
    public void Configure(EntityTypeBuilder<TrackingEvent> builder)
    {
        builder.ToTable("TrackingEvents");

        builder.HasKey(te => te.Id);

        builder.Property(te => te.Timestamp)
            .IsRequired();

        builder.Property(te => te.EventType)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(te => te.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(te => te.Location)
            .HasMaxLength(200);

        builder.Property(te => te.Operator)
            .HasMaxLength(200);

        builder.Property(te => te.DelayReason)
            .HasConversion<string>();

        builder.HasOne(te => te.Parcel)
            .WithMany(p => p.TrackingEvents)
            .HasForeignKey(te => te.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(te => te.Timestamp);
        builder.HasIndex(te => new { te.ParcelId, te.Timestamp });
    }
}
