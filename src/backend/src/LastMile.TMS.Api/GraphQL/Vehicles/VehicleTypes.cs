using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Vehicles;

public sealed class VehicleType : EntityObjectType<Vehicle>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<Vehicle> descriptor)
    {
        descriptor.Name("Vehicle");
        descriptor.Field(v => v.Id).IsProjected(true);
        descriptor.Field(v => v.RegistrationPlate);
        descriptor.Field(v => v.Type);
        descriptor.Field(v => v.ParcelCapacity);
        descriptor.Field(v => v.WeightCapacity);
        descriptor.Field(v => v.Status);
        descriptor.Field(v => v.DepotId).IsProjected(true);
        descriptor.Field("depotName")
            .Type<StringType>()
            .Resolve(async ctx =>
            {
                var vehicle = ctx.Parent<Vehicle>();
                return await LoadDepotNameAsync(ctx, vehicle.DepotId);
            });
        descriptor.Field("totalRoutes")
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx => (await LoadStatsAsync(ctx) ?? VehicleRouteStats.Empty(ctx.Parent<Vehicle>().Id)).TotalRoutes);
        descriptor.Field("routesCompleted")
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx => (await LoadStatsAsync(ctx) ?? VehicleRouteStats.Empty(ctx.Parent<Vehicle>().Id)).RoutesCompleted);
        descriptor.Field("totalMileage")
            .Type<NonNullType<IntType>>()
            .Resolve(async ctx => (await LoadStatsAsync(ctx) ?? VehicleRouteStats.Empty(ctx.Parent<Vehicle>().Id)).TotalMileage);
        descriptor.Field(v => v.CreatedAt);
        descriptor.Field(v => v.LastModifiedAt).Name("updatedAt");
    }

    private static Task<string?> LoadDepotNameAsync(IResolverContext ctx, Guid depotId) =>
        ctx.BatchDataLoader<Guid, string?>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var depots = await dbContext.Depots
                        .AsNoTracking()
                        .Where(d => ids.Contains(d.Id))
                        .Select(d => new { d.Id, d.Name })
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id => depots.FirstOrDefault(d => d.Id == id)?.Name);
                })
            .LoadAsync(depotId);

    private static Task<VehicleRouteStats?> LoadStatsAsync(IResolverContext ctx)
    {
        var vehicleId = ctx.Parent<Vehicle>().Id;

        return ctx.BatchDataLoader<Guid, VehicleRouteStats>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var stats = await dbContext.Routes
                        .AsNoTracking()
                        .Where(r => ids.Contains(r.VehicleId))
                        .GroupBy(r => r.VehicleId)
                        .Select(g => new VehicleRouteStats(
                            g.Key,
                            g.Count(),
                            g.Count(r => r.Status == RouteStatus.Completed),
                            g.Sum(r => r.Status == RouteStatus.Completed && r.EndMileage > 0
                                ? r.EndMileage - r.StartMileage
                                : 0)))
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id => stats.FirstOrDefault(s => s.VehicleId == id) ?? VehicleRouteStats.Empty(id));
                })
            .LoadAsync(vehicleId);
    }

    private sealed record VehicleRouteStats(Guid VehicleId, int TotalRoutes, int RoutesCompleted, int TotalMileage)
    {
        public static VehicleRouteStats Empty(Guid vehicleId) => new(vehicleId, 0, 0, 0);
    }
}

public sealed class VehicleFilterInputType : FilterInputType<Vehicle>
{
    protected override void Configure(IFilterInputTypeDescriptor<Vehicle> descriptor)
    {
        descriptor.Name("VehicleFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(v => v.Id);
        descriptor.Field(v => v.RegistrationPlate);
        descriptor.Field(v => v.Type);
        descriptor.Field(v => v.ParcelCapacity);
        descriptor.Field(v => v.WeightCapacity);
        descriptor.Field(v => v.Status);
        descriptor.Field(v => v.DepotId);
        descriptor.Field(v => v.CreatedAt);
        descriptor.Field(v => v.LastModifiedAt).Name("updatedAt");
    }
}

public sealed class VehicleSortInputType : SortInputType<Vehicle>
{
    protected override void Configure(ISortInputTypeDescriptor<Vehicle> descriptor)
    {
        descriptor.Name("VehicleSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(v => v.Id);
        descriptor.Field(v => v.RegistrationPlate);
        descriptor.Field(v => v.Type);
        descriptor.Field(v => v.ParcelCapacity);
        descriptor.Field(v => v.WeightCapacity);
        descriptor.Field(v => v.Status);
        descriptor.Field(v => v.DepotId);
        descriptor.Field(v => v.CreatedAt);
        descriptor.Field(v => v.LastModifiedAt).Name("updatedAt");
    }
}
