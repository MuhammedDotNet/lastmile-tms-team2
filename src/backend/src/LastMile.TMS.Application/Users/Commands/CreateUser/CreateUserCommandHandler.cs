using System.Security.Cryptography;
using FluentValidation;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Users.Mappings;
using LastMile.TMS.Application.Users.Support;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class CreateUserCommandHandler(
    IAppDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IUserAccountEmailJobScheduler emailJobScheduler,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateUserCommand, ApplicationUser>
{
    public async Task<ApplicationUser> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        await UserManagementRules.EnsureValidAssignmentsAsync(
            dbContext,
            request.Dto.DepotId,
            request.Dto.ZoneId,
            cancellationToken);

        var email = request.Dto.Email.Trim();
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            throw UserManagementRules.CreateValidationException("A user with this email already exists.");
        }

        var roleName = UserManagementRoleHelper.ToIdentityRoleName(request.Dto.Role);
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            throw UserManagementRules.CreateValidationException("The selected role does not exist.");
        }

        var temporaryPassword = GenerateTemporaryPassword();
        var user = request.Dto.ToEntity();
        user.FirstName = request.Dto.FirstName.Trim();
        user.LastName = request.Dto.LastName.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = NormalizeOptional(request.Dto.Phone);
        user.IsActive = true;
        user.CreatedAt = DateTimeOffset.UtcNow;
        user.CreatedBy = currentUserService.UserId ?? "system";

        var createResult = await userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            throw CreateIdentityValidationException(createResult);
        }

        var roleResult = await userManager.AddToRoleAsync(user, roleName);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            throw CreateIdentityValidationException(roleResult);
        }

        await emailJobScheduler.SchedulePasswordSetupEmailAsync(user.Id, cancellationToken);

        return user;
    }

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static ValidationException CreateIdentityValidationException(IdentityResult result) =>
        UserManagementRules.CreateValidationException(result.Errors.Select(x => x.Description).ToArray());

    private static string GenerateTemporaryPassword()
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string allCharacters = uppercase + lowercase + digits;

        Span<char> password = stackalloc char[16];
        password[0] = uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)];
        password[1] = lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)];
        password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];

        for (var index = 3; index < password.Length; index++)
        {
            password[index] = allCharacters[RandomNumberGenerator.GetInt32(allCharacters.Length)];
        }

        for (var index = password.Length - 1; index > 0; index--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
            (password[index], password[swapIndex]) = (password[swapIndex], password[index]);
        }

        return new string(password);
    }
}
