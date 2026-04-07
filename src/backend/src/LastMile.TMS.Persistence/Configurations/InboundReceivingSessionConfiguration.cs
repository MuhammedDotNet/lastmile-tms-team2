using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class InboundReceivingSessionConfiguration : IEntityTypeConfiguration<InboundReceivingSession>
{
    public void Configure(EntityTypeBuilder<InboundReceivingSession> builder)
    {
        builder.ToTable("InboundReceivingSessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.ConfirmedBy)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.ManifestId, x.Status });

        builder.HasMany(x => x.Scans)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Exceptions)
            .WithOne(x => x.Session)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
