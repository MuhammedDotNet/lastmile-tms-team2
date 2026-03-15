using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelContentItemConfiguration : IEntityTypeConfiguration<ParcelContentItem>
{
    public void Configure(EntityTypeBuilder<ParcelContentItem> builder)
    {
        builder.ToTable("ParcelContentItems");

        builder.HasKey(pci => pci.Id);

        builder.Property(pci => pci.HsCode)
            .IsRequired()
            .HasMaxLength(7)
            .IsFixedLength();

        builder.Property(pci => pci.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(pci => pci.Quantity)
            .IsRequired();

        builder.Property(pci => pci.UnitValue)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(pci => pci.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("USD");

        builder.Property(pci => pci.Weight)
            .HasPrecision(18, 4);

        builder.Property(pci => pci.WeightUnit)
            .HasConversion<string>();

        builder.Property(pci => pci.CountryOfOrigin)
            .HasMaxLength(3);

        builder.HasOne(pci => pci.Parcel)
            .WithMany(p => p.ContentItems)
            .HasForeignKey(pci => pci.ParcelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pci => new { pci.ParcelId, pci.HsCode });
    }
}
