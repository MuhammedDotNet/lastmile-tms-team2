using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelChangeHistoryEntryConfiguration : IEntityTypeConfiguration<ParcelChangeHistoryEntry>
{
    public void Configure(EntityTypeBuilder<ParcelChangeHistoryEntry> builder)
    {
        builder.ToTable("ParcelChangeHistoryEntries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Action)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(entry => entry.FieldName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(entry => entry.BeforeValue)
            .HasMaxLength(2000);

        builder.Property(entry => entry.AfterValue)
            .HasMaxLength(2000);

        builder.Property(entry => entry.ChangedAt)
            .IsRequired();

        builder.Property(entry => entry.ChangedBy)
            .HasMaxLength(256);

        builder.HasOne(entry => entry.Parcel)
            .WithMany(parcel => parcel.ChangeHistory)
            .HasForeignKey(entry => entry.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(entry => new { entry.ParcelId, entry.ChangedAt });
    }
}
