using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.AspNetCore.Authorization;
using LastMile.TMS.Application.Users.Commands;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using MediatR;

namespace LastMile.TMS.Api.GraphQL.Users;

[ExtendObjectType(OperationTypeNames.Mutation)]
[Authorize(Roles = new[] { nameof(PredefinedRole.Admin) })]
public sealed class UserManagementMutations
{
    public Task<ApplicationUser> CreateUser(
        CreateUserInput input,
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(new CreateUserCommand(input.ToDto()), cancellationToken);

    public Task<ApplicationUser> UpdateUser(
        UpdateUserInput input,
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(new UpdateUserCommand(input.Id, input.ToDto()), cancellationToken);

    public Task<ApplicationUser> DeactivateUser(
        Guid userId,
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(new DeactivateUserCommand(userId), cancellationToken);

    public Task<UserActionResultDto> SendPasswordResetEmail(
        Guid userId,
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(new SendPasswordResetEmailCommand(userId), cancellationToken);

    [AllowAnonymous]
    public Task<UserActionResultDto> RequestPasswordReset(
        string email,
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(new RequestPasswordResetCommand(email), cancellationToken);

    [AllowAnonymous]
    public Task<UserActionResultDto> CompletePasswordReset(
        CompletePasswordResetInput input,
        [Service] ISender sender,
        CancellationToken cancellationToken) =>
        sender.Send(
            new CompletePasswordResetCommand(
                input.Email,
                input.Token,
                input.NewPassword),
            cancellationToken);
}
