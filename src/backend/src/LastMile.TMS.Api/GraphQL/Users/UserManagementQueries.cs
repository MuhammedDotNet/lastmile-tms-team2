using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Application.Users.Queries;
using LastMile.TMS.Application.Users.Reads;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Users;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class UserManagementQueries
{
    [Authorize(Roles = new[] { nameof(PredefinedRole.Admin) })]
    [UseProjection]
    [UseFiltering(typeof(UserManagementUserFilterInputType))]
    [UseSorting(typeof(UserManagementUserSortInputType))]
    public IQueryable<ApplicationUser> Users(
        string? search = null,
        bool? isActive = null,
        Guid? depotId = null,
        Guid? zoneId = null,
        [Service] IUserReadService readService = null!) =>
        readService.GetUsers(search, isActive, depotId, zoneId);

    [Authorize(Roles = new[] { nameof(PredefinedRole.Admin) })]
    [UseFirstOrDefault]
    [UseProjection]
    public IQueryable<ApplicationUser> User(
        Guid id,
        [Service] IUserReadService readService = null!) =>
        readService.GetUsers().Where(u => u.Id == id);

    [Authorize(Roles = new[] { nameof(PredefinedRole.Admin) })]
    public Task<UserManagementLookupsDto> UserManagementLookups(
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(new GetUserManagementLookupsQuery(), cancellationToken);
}
