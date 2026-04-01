using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class CreateParcelImportCommandHandler(
    IAppDbContext db,
    IParcelImportFileParser parser,
    IParcelImportJobScheduler scheduler,
    ICurrentUserService currentUser)
    : IRequestHandler<CreateParcelImportCommand, Guid>
{
    public async Task<Guid> Handle(CreateParcelImportCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        _ = await parser.ParseAsync(dto.FileName, dto.SourceFile, cancellationToken);

        var parcelImport = new ParcelImport
        {
            FileName = dto.FileName,
            FileFormat = dto.FileFormat,
            ShipperAddressId = dto.ShipperAddressId,
            Status = ParcelImportStatus.Queued,
            SourceFile = dto.SourceFile,
            CreatedBy = currentUser.UserName ?? currentUser.UserId,
        };

        db.ParcelImports.Add(parcelImport);
        await db.SaveChangesAsync(cancellationToken);

        await scheduler.ScheduleAsync(parcelImport.Id, cancellationToken);
        return parcelImport.Id;
    }
}
