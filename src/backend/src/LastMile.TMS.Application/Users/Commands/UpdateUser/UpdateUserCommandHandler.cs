using FluentValidation;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Users.Mappings;
using LastMile.TMS.Application.Users.Support;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class UpdateUserCommandHandler(
    IAppDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ICurrentUserService currentUserService)
    : IRequestHandler<UpdateUserCommand, ApplicationUser>
{
    public async Task<ApplicationUser> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        await UserManagementRules.EnsureValidAssignmentsAsync(
            dbContext,
            request.Dto.DepotId,
            request.Dto.ZoneId,
            cancellationToken);

        var user = await userManager.FindByIdAsync(request.Id.ToString());
        if (user is null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        UserManagementRules.EnsureUserCanBeManaged(user);

        var email = request.Dto.Email.Trim();
        var emailOwner = await userManager.FindByEmailAsync(email);
        if (emailOwner is not null && emailOwner.Id != user.Id)
        {
            throw UserManagementRules.CreateValidationException("A user with this email already exists.");
        }

        var roleName = UserManagementRoleHelper.ToIdentityRoleName(request.Dto.Role);
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            throw UserManagementRules.CreateValidationException("The selected role does not exist.");
        }

        request.Dto.UpdateEntity(user);
        user.FirstName = request.Dto.FirstName.Trim();
        user.LastName = request.Dto.LastName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = NormalizeOptional(request.Dto.Phone);
        user.LastModifiedAt = DateTimeOffset.UtcNow;
        user.LastModifiedBy = currentUserService.UserId ?? "system";

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw CreateIdentityValidationException(updateResult);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count != 1 || !currentRoles.Contains(roleName))
        {
            if (currentRoles.Count > 0)
            {
                var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    throw CreateIdentityValidationException(removeResult);
                }
            }

            var addResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addResult.Succeeded)
            {
                throw CreateIdentityValidationException(addResult);
            }
        }

        return user;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ValidationException CreateIdentityValidationException(IdentityResult result) =>
        UserManagementRules.CreateValidationException(result.Errors.Select(x => x.Description).ToArray());
}
