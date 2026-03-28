using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.GraphQL.Parcels;

public sealed class ParcelType : ObjectType<ParcelDto>
{
    protected override void Configure(IObjectTypeDescriptor<ParcelDto> descriptor)
    {
        descriptor.Name("RegisteredParcel");
        descriptor.BindFieldsImplicitly();
    }
}

public sealed class ParcelRouteOptionType : EntityObjectType<Parcel>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<Parcel> descriptor)
    {
        descriptor.Name("ParcelRouteOption");
        descriptor.Field(p => p.Id);
        descriptor.Field(p => p.TrackingNumber);
        descriptor.Field(p => p.Weight);
        descriptor.Field(p => p.WeightUnit);
    }
}
