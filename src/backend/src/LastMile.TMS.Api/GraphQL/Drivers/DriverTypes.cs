using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.Queries;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Drivers;

public sealed class DriverType : EntityObjectType<Driver>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<Driver> descriptor)
    {
        descriptor.Name("Driver");
        descriptor.Field(d => d.Id).IsProjected(true);
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
        descriptor.Field(d => d.LicenseNumber).IsProjected(true);
        descriptor.Field(d => d.LicenseExpiryDate);
        descriptor.Field(d => d.PhotoUrl);
        descriptor.Field(d => d.ZoneId).IsProjected(true);
        descriptor.Field(d => d.DepotId).IsProjected(true);
        descriptor.Field(d => d.Status).IsProjected(true);
        descriptor.Field(d => d.UserId).IsProjected(true);
        // Names resolved by Driver.Id so UseProjection can materialize partial entities without ZoneId/DepotId/UserId.
        descriptor.Field("zoneName")
            .Type<StringType>()
            .Resolve(async ctx => (await LoadDriverLabelsAsync(ctx, ctx.Parent<Driver>().Id)).ZoneName);
        descriptor.Field("depotName")
            .Type<StringType>()
            .Resolve(async ctx => (await LoadDriverLabelsAsync(ctx, ctx.Parent<Driver>().Id)).DepotName);
        descriptor.Field("userName")
            .Type<StringType>()
            .Resolve(async ctx => (await LoadDriverLabelsAsync(ctx, ctx.Parent<Driver>().Id)).UserName);
        descriptor.Field("availabilitySchedule")
            .Type<ListType<NonNullType<DriverAvailabilityType>>>()
            .Resolve(async ctx =>
            {
                var driver = ctx.Parent<Driver>();
                return await LoadAvailabilitiesAsync(ctx, driver.Id);
            });
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt).Name("updatedAt");
    }

    private sealed record DriverLabels(string? ZoneName, string? DepotName, string? UserName);

    private static async Task<DriverLabels> LoadDriverLabelsAsync(
        IResolverContext ctx,
        Guid driverId)
    {
        var labels = await ctx.BatchDataLoader<Guid, DriverLabels>(
                async (driverIds, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var rows = await dbContext.Drivers
                        .AsNoTracking()
                        .Where(d => driverIds.Contains(d.Id))
                        .Select(d => new
                        {
                            d.Id,
                            ZoneName = d.Zone.Name,
                            DepotName = d.Depot.Name,
                            UserName = d.User.UserName,
                        })
                        .ToListAsync(ct);

                    return driverIds.ToDictionary(
                        id => id,
                        id =>
                        {
                            var row = rows.FirstOrDefault(r => r.Id == id);
                            return row is null
                                ? new DriverLabels(null, null, null)
                                : new DriverLabels(row.ZoneName, row.DepotName, row.UserName);
                        });
                },
                "DriverLabelsByDriverId")
            .LoadAsync(driverId);

        return labels ?? new DriverLabels(null, null, null);
    }

    private static async Task<List<DriverAvailability>> LoadAvailabilitiesAsync(
        IResolverContext ctx,
        Guid driverId)
    {
        var list = await ctx.BatchDataLoader<Guid, List<DriverAvailability>>(
                async (driverIds, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var availabilities = await dbContext.DriverAvailabilities
                        .AsNoTracking()
                        .Where(a => driverIds.Contains(a.DriverId))
                        .ToListAsync(ct);

                    return driverIds.ToDictionary(
                        id => id,
                        id => availabilities.Where(a => a.DriverId == id).ToList());
                },
                "DriverAvailabilitiesByDriverId")
            .LoadAsync(driverId);

        return list ?? [];
    }
}

public sealed class DriverAvailabilityType : EntityObjectType<DriverAvailability>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<DriverAvailability> descriptor)
    {
        descriptor.Name("DriverAvailability");
        descriptor.Field(a => a.Id);
        descriptor.Field(a => a.DayOfWeek);
        descriptor.Field(a => a.ShiftStart);
        descriptor.Field(a => a.ShiftEnd);
        descriptor.Field(a => a.IsAvailable);
    }
}

public sealed class DriverFilterInputType : FilterInputType<Driver>
{
    protected override void Configure(IFilterInputTypeDescriptor<Driver> descriptor)
    {
        descriptor.Name("DriverFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.FirstName);
        descriptor.Field(d => d.LastName);
        descriptor.Field(d => d.Phone);
        descriptor.Field(d => d.Email);
        descriptor.Field(d => d.LicenseNumber);
        descriptor.Field(d => d.LicenseExpiryDate);
        descriptor.Field(d => d.ZoneId);
        descriptor.Field(d => d.DepotId);
        descriptor.Field(d => d.Status);
        descriptor.Field(d => d.UserId);
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt).Name("updatedAt");
    }
}

public sealed class DriverSortInputType : SortInputType<Driver>
{
    protected override void Configure(ISortInputTypeDescriptor<Driver> descriptor)
    {
        descriptor.Name("DriverSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(d => d.Id);
        descriptor.Field(d => d.FirstName);
        descriptor.Field(d => d.LastName);
        descriptor.Field(d => d.Phone);
        descriptor.Field(d => d.Email);
        descriptor.Field(d => d.LicenseNumber);
        descriptor.Field(d => d.LicenseExpiryDate);
        descriptor.Field(d => d.ZoneId);
        descriptor.Field(d => d.DepotId);
        descriptor.Field(d => d.Status);
        descriptor.Field(d => d.UserId);
        descriptor.Field(d => d.CreatedAt);
        descriptor.Field(d => d.LastModifiedAt).Name("updatedAt");
    }
}
