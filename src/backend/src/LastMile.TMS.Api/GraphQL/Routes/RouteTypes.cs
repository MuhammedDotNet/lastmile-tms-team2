using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using RouteEntity = LastMile.TMS.Domain.Entities.Route;

namespace LastMile.TMS.Api.GraphQL.Routes;

public sealed class RouteType : EntityObjectType<RouteEntity>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<RouteEntity> descriptor)
    {
        descriptor.Name("Route");
        descriptor.Field(r => r.Id).IsProjected(true);
        descriptor.Field(r => r.VehicleId).IsProjected(true);
        descriptor.Field("vehiclePlate")
            .Type<StringType>()
            .Resolve(async ctx => await LoadVehiclePlateAsync(ctx, ctx.Parent<RouteEntity>().VehicleId));
        descriptor.Field(r => r.DriverId).IsProjected(true);
        descriptor.Field("driverName")
            .Type<StringType>()
            .Resolve(async ctx => await LoadDriverNameAsync(ctx, ctx.Parent<RouteEntity>().DriverId));
        descriptor.Field(r => r.StartDate);
        descriptor.Field(r => r.EndDate);
        descriptor.Field(r => r.StartMileage);
        descriptor.Field(r => r.EndMileage);
        descriptor.Field("totalMileage")
            .Type<NonNullType<IntType>>()
            .Resolve(ctx =>
            {
                var route = ctx.Parent<RouteEntity>();
                return route.EndMileage > 0 ? route.EndMileage - route.StartMileage : 0;
            });
        descriptor.Field(r => r.Status);
        descriptor.Field("parcelCount")
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx => (await LoadParcelStatsAsync(ctx) ?? RouteParcelStats.Empty(ctx.Parent<RouteEntity>().Id)).ParcelCount);
        descriptor.Field("parcelsDelivered")
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx => (await LoadParcelStatsAsync(ctx) ?? RouteParcelStats.Empty(ctx.Parent<RouteEntity>().Id)).ParcelsDelivered);
        descriptor.Field(r => r.CreatedAt);
    }

    private static Task<string?> LoadVehiclePlateAsync(IResolverContext ctx, Guid vehicleId) =>
        ctx.BatchDataLoader<Guid, string?>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var vehicles = await dbContext.Vehicles
                        .AsNoTracking()
                        .Where(v => ids.Contains(v.Id))
                        .Select(v => new { v.Id, v.RegistrationPlate })
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id => vehicles.FirstOrDefault(v => v.Id == id)?.RegistrationPlate);
                })
            .LoadAsync(vehicleId);

    private static Task<string?> LoadDriverNameAsync(IResolverContext ctx, Guid driverId) =>
        ctx.BatchDataLoader<Guid, string?>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var drivers = await dbContext.Drivers
                        .AsNoTracking()
                        .Where(d => ids.Contains(d.Id))
                        .Select(d => new { d.Id, d.FirstName, d.LastName })
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id =>
                        {
                            var driver = drivers.FirstOrDefault(d => d.Id == id);
                            return driver is null ? null : $"{driver.FirstName} {driver.LastName}".Trim();
                        });
                })
            .LoadAsync(driverId);

    private static Task<RouteParcelStats?> LoadParcelStatsAsync(IResolverContext ctx)
    {
        var routeId = ctx.Parent<RouteEntity>().Id;

        return ctx.BatchDataLoader<Guid, RouteParcelStats>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var stats = await dbContext.Routes
                        .AsNoTracking()
                        .Where(r => ids.Contains(r.Id))
                        .Select(r => new RouteParcelStats(
                            r.Id,
                            r.Parcels.Count,
                            r.Parcels.Count(p => p.Status == ParcelStatus.Delivered)))
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id => stats.FirstOrDefault(s => s.RouteId == id) ?? RouteParcelStats.Empty(id));
                })
            .LoadAsync(routeId);
    }

    private sealed record RouteParcelStats(Guid RouteId, int ParcelCount, int ParcelsDelivered)
    {
        public static RouteParcelStats Empty(Guid routeId) => new(routeId, 0, 0);
    }
}

public sealed class RouteFilterInputType : FilterInputType<RouteEntity>
{
    protected override void Configure(IFilterInputTypeDescriptor<RouteEntity> descriptor)
    {
        descriptor.Name("RouteFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(r => r.Id);
        descriptor.Field(r => r.VehicleId);
        descriptor.Field(r => r.DriverId);
        descriptor.Field(r => r.StartDate);
        descriptor.Field(r => r.EndDate);
        descriptor.Field(r => r.StartMileage);
        descriptor.Field(r => r.EndMileage);
        descriptor.Field(r => r.Status);
        descriptor.Field(r => r.CreatedAt);
    }
}

public sealed class RouteSortInputType : SortInputType<RouteEntity>
{
    protected override void Configure(ISortInputTypeDescriptor<RouteEntity> descriptor)
    {
        descriptor.Name("RouteSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(r => r.Id);
        descriptor.Field(r => r.VehicleId);
        descriptor.Field(r => r.DriverId);
        descriptor.Field(r => r.StartDate);
        descriptor.Field(r => r.EndDate);
        descriptor.Field(r => r.StartMileage);
        descriptor.Field(r => r.EndMileage);
        descriptor.Field(r => r.Status);
        descriptor.Field(r => r.CreatedAt);
    }
}
