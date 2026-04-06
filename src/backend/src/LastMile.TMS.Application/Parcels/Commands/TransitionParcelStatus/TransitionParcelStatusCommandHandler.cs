using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Mappings;
using LastMile.TMS.Application.Parcels.Support;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Application.Parcels.Commands;

public sealed class TransitionParcelStatusCommandHandler(
    IAppDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<TransitionParcelStatusCommand, ParcelDto>
{
    public async Task<ParcelDto> Handle(TransitionParcelStatusCommand request, CancellationToken cancellationToken)
    {
        var parcel = await dbContext.Parcels
            .Include(p => p.TrackingEvents)
            .Include(p => p.Zone)
            .ThenInclude(z => z!.Depot)
            .FirstOrDefaultAsync(p => p.Id == request.ParcelId, cancellationToken);

        if (parcel is null)
        {
            throw new InvalidOperationException($"Parcel with ID '{request.ParcelId}' was not found.");
        }

        parcel.TransitionTo(request.NewStatus);

        var actor = currentUser.UserName ?? currentUser.UserId ?? "System";
        var now = DateTimeOffset.UtcNow;
        var trackingEvent = ParcelTrackingEventFactory.CreateForParcelStatus(
            parcel.Id,
            request.NewStatus,
            now,
            request.Location,
            request.Description,
            actor);

        parcel.TrackingEvents.Add(trackingEvent);

        await dbContext.SaveChangesAsync(cancellationToken);

        // Use a projection query to ensure Zone and Depot are loaded before mapping
        var dto = await dbContext.Parcels
            .AsNoTracking()
            .Where(p => p.Id == parcel.Id)
            .Select(p => new ParcelDto
            {
                Id = p.Id,
                TrackingNumber = p.TrackingNumber,
                Barcode = p.TrackingNumber,
                Description = p.Description,
                ServiceType = p.ServiceType.ToString(),
                Status = p.Status.ToString(),
                Weight = p.Weight,
                WeightUnit = p.WeightUnit.ToString(),
                Length = p.Length,
                Width = p.Width,
                Height = p.Height,
                DimensionUnit = p.DimensionUnit.ToString(),
                DeclaredValue = p.DeclaredValue,
                Currency = p.Currency,
                EstimatedDeliveryDate = p.EstimatedDeliveryDate,
                ActualDeliveryDate = p.ActualDeliveryDate,
                DeliveryAttempts = p.DeliveryAttempts,
                ParcelType = p.ParcelType,
                ZoneId = p.Zone.Id,
                ZoneName = p.Zone.Name,
                DepotId = p.Zone.Depot.Id,
                DepotName = p.Zone.Depot.Name,
                CreatedAt = p.CreatedAt,
                LastModifiedAt = p.LastModifiedAt,
                RecipientContactName = p.RecipientAddress.ContactName,
                RecipientCompanyName = p.RecipientAddress.CompanyName,
                RecipientStreet1 = p.RecipientAddress.Street1,
                RecipientCity = p.RecipientAddress.City,
                RecipientPostalCode = p.RecipientAddress.PostalCode
            })
            .FirstAsync(cancellationToken);

        return dto;
    }
}
