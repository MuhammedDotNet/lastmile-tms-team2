using LastMile.TMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastMile.TMS.Persistence.Configurations;

public class ParcelImportRowFailureConfiguration : IEntityTypeConfiguration<ParcelImportRowFailure>
{
    public void Configure(EntityTypeBuilder<ParcelImportRowFailure> builder)
    {
        builder.ToTable("ParcelImportRowFailures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RowNumber)
            .IsRequired();

        builder.Property(x => x.OriginalRowValues)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(x => new { x.ParcelImportId, x.RowNumber });
    }
}
