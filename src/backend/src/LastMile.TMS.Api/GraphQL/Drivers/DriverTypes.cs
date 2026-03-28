using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Api.GraphQL.Drivers;

public sealed class DriverType : EntityObjectType<Driver>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<Driver> descriptor)
    {
        descriptor.Name("Driver");
        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.FirstName).IsProjected(true);
        descriptor.Field(d => d.LastName).IsProjected(true);
        descriptor.Field("displayName")
            .Type<NonNullType<StringType>>()
            .Resolve(ctx =>
            {
                var driver = ctx.Parent<Driver>();
                return $"{driver.FirstName} {driver.LastName}".Trim();
            });
        descriptor.Field(d => d.Phone);
        descriptor.Field(d => d.Email);
        descriptor.Field(d => d.DepotId);
        descriptor.Field(d => d.Status);
    }
}
