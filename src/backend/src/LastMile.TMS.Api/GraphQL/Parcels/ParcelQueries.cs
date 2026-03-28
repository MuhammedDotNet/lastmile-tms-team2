using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Parcels.Reads;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.GraphQL.Parcels;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class ParcelQueries
{
    [Authorize(Roles = new[] { "OperationsManager", "Admin", "Dispatcher" })]
    [UseProjection]
    public IQueryable<Parcel> GetParcelsForRouteCreation(
        [Service] IParcelReadService readService = null!) =>
        readService.GetParcelsForRouteCreation();
}
