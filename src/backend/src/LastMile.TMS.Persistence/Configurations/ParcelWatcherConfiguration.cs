using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelWatcherConfiguration : IEntityTypeConfiguration<ParcelWatcher>
{
    public void Configure(EntityTypeBuilder<ParcelWatcher> builder)
    {
        builder.ToTable("ParcelWatchers");

        builder.HasKey(pw => pw.Id);

        builder.Property(pw => pw.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(pw => pw.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(pw => pw.Email)            
            .IsUnique();

        builder.HasMany(pw => pw.Parcels)
            .WithMany(p => p.Watchers)
            .UsingEntity<Dictionary<string, object>>(
                "ParcelWatcherLinks",
                right => right.HasOne<Parcel>().WithMany().HasForeignKey("ParcelId").HasConstraintName("FK_ParcelWatcherLinks_Parcels"),
                left => left.HasOne<ParcelWatcher>().WithMany().HasForeignKey("ParcelWatcherId").HasConstraintName("FK_ParcelWatcherLinks_ParcelWatchers"));
    }
}
