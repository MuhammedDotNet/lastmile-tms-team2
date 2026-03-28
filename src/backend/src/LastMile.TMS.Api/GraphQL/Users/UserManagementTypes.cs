using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using LastMile.TMS.Api.GraphQL.Common;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LastMile.TMS.Api.GraphQL.Users;

public sealed class UserRoleType : EnumType<PredefinedRole>
{
    protected override void Configure(IEnumTypeDescriptor<PredefinedRole> descriptor)
    {
        descriptor.Name("UserRole");

        descriptor.Value(PredefinedRole.Admin).Name(nameof(PredefinedRole.Admin));
        descriptor.Value(PredefinedRole.OperationsManager).Name(nameof(PredefinedRole.OperationsManager));
        descriptor.Value(PredefinedRole.Dispatcher).Name(nameof(PredefinedRole.Dispatcher));
        descriptor.Value(PredefinedRole.WarehouseOperator).Name(nameof(PredefinedRole.WarehouseOperator));
        descriptor.Value(PredefinedRole.Driver).Name(nameof(PredefinedRole.Driver));
    }
}

public sealed class UserManagementUserType : EntityObjectType<ApplicationUser>
{
    protected override void ConfigureFields(IObjectTypeDescriptor<ApplicationUser> descriptor)
    {
        descriptor.Name("UserManagementUser");
        descriptor.Field(u => u.Id).IsProjected(true);
        descriptor.Field(u => u.FirstName).IsProjected(true);
        descriptor.Field(u => u.LastName).IsProjected(true);
        descriptor.Field("fullName")
            .Type<NonNullType<StringType>>()
            .Resolve(ctx =>
            {
                var user = ctx.Parent<ApplicationUser>();
                return $"{user.FirstName} {user.LastName}".Trim();
            });
        descriptor.Field(u => u.Email);
        descriptor.Field(u => u.PhoneNumber).Name("phone");
        descriptor.Field("role")
            .Type<StringType>()
            .Resolve(async ctx => await LoadRoleAsync(ctx, ctx.Parent<ApplicationUser>().Id));
        descriptor.Field(u => u.IsActive);
        descriptor.Field(u => u.IsSystemAdmin)
            .IsProjected(true)
            .Name("isProtected")
            .Type<NonNullType<BooleanType>>()
            .Resolve(ctx => ctx.Parent<ApplicationUser>().IsSystemAdmin);
        descriptor.Field(u => u.DepotId).IsProjected(true);
        descriptor.Field("depotName")
            .Type<StringType>()
            .Resolve(async ctx =>
            {
                var user = ctx.Parent<ApplicationUser>();
                return user.DepotId is null ? null : await LoadDepotNameAsync(ctx, user.DepotId.Value);
            });
        descriptor.Field(u => u.ZoneId).IsProjected(true);
        descriptor.Field("zoneName")
            .Type<StringType>()
            .Resolve(async ctx =>
            {
                var user = ctx.Parent<ApplicationUser>();
                return user.ZoneId is null ? null : await LoadZoneNameAsync(ctx, user.ZoneId.Value);
            });
        descriptor.Field(u => u.CreatedAt);
        descriptor.Field(u => u.LastModifiedAt).Name("updatedAt");
    }

    private static Task<string?> LoadRoleAsync(IResolverContext ctx, Guid userId) =>
        ctx.BatchDataLoader<Guid, string?>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var roles = await dbContext.UserRoles
                        .Join(dbContext.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Name })
                        .Where(x => x.Name != null && ids.Contains(x.UserId))
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id => roles.FirstOrDefault(x => x.UserId == id)?.Name);
                },
                "UserManagementRoleByUserId")
            .LoadAsync(userId);

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
                },
                "UserManagementDepotNameById")
            .LoadAsync(depotId);

    private static Task<string?> LoadZoneNameAsync(IResolverContext ctx, Guid zoneId) =>
        ctx.BatchDataLoader<Guid, string?>(
                async (ids, ct) =>
                {
                    var dbContext = ctx.Service<IAppDbContext>();
                    var zones = await dbContext.Zones
                        .AsNoTracking()
                        .Where(z => ids.Contains(z.Id))
                        .Select(z => new { z.Id, z.Name })
                        .ToListAsync(ct);

                    return ids.ToDictionary(
                        id => id,
                        id => zones.FirstOrDefault(z => z.Id == id)?.Name);
                },
                "UserManagementZoneNameById")
            .LoadAsync(zoneId);
}

public sealed class UserManagementUserFilterInputType : FilterInputType<ApplicationUser>
{
    protected override void Configure(IFilterInputTypeDescriptor<ApplicationUser> descriptor)
    {
        descriptor.Name("UserManagementUserFilterInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(u => u.Id);
        descriptor.Field(u => u.FirstName);
        descriptor.Field(u => u.LastName);
        descriptor.Field(u => u.Email);
        descriptor.Field(u => u.PhoneNumber).Name("phone");
        descriptor.Field(u => u.IsActive);
        descriptor.Field(u => u.DepotId);
        descriptor.Field(u => u.ZoneId);
        descriptor.Field(u => u.CreatedAt);
        descriptor.Field(u => u.LastModifiedAt).Name("updatedAt");
    }
}

public sealed class UserManagementUserSortInputType : SortInputType<ApplicationUser>
{
    protected override void Configure(ISortInputTypeDescriptor<ApplicationUser> descriptor)
    {
        descriptor.Name("UserManagementUserSortInput");
        descriptor.BindFieldsExplicitly();
        descriptor.Field(u => u.Id);
        descriptor.Field(u => u.FirstName);
        descriptor.Field(u => u.LastName);
        descriptor.Field(u => u.Email);
        descriptor.Field(u => u.PhoneNumber).Name("phone");
        descriptor.Field(u => u.IsActive);
        descriptor.Field(u => u.DepotId);
        descriptor.Field(u => u.ZoneId);
        descriptor.Field(u => u.CreatedAt);
        descriptor.Field(u => u.LastModifiedAt).Name("updatedAt");
    }
}
