using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Street1)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.Street2)
            .HasMaxLength(500);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PostalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.CountryCode)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("US");

        builder.Property(a => a.IsResidential)
            .HasDefaultValue(false);

        builder.Property(a => a.ContactName)
            .HasMaxLength(200);

        builder.Property(a => a.CompanyName)
            .HasMaxLength(200);

        builder.Property(a => a.Phone)
            .HasMaxLength(50);

        builder.Property(a => a.Email)
            .HasMaxLength(200);

        builder.HasIndex(a => a.PostalCode);
        builder.HasIndex(a => a.City);
        builder.HasIndex(a => new { a.City, a.State });

        builder.Ignore(a => a.ShipperParcels);
        builder.Ignore(a => a.RecipientParcels);
    }
}