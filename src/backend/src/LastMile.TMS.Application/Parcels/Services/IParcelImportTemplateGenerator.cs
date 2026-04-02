using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Parcels.Services;

public interface IParcelImportTemplateGenerator
{
    byte[] GenerateTemplate(ParcelImportFileFormat format);
}
