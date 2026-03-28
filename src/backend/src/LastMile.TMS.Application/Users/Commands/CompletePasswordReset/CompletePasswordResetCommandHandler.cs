using System.Text;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Application.Users.Support;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class CompletePasswordResetCommandHandler(UserManager<ApplicationUser> userManager)
    : IRequestHandler<CompletePasswordResetCommand, UserActionResultDto>
{
    public async Task<UserActionResultDto> Handle(
        CompletePasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            throw UserManagementRules.CreateValidationException("The password reset link is invalid.");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
        }
        catch (Exception)
        {
            throw UserManagementRules.CreateValidationException("The password reset link is invalid.");
        }

        var result = await userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
        {
            throw UserManagementRules.CreateValidationException(result.Errors.Select(x => x.Description).ToArray());
        }

        return new UserActionResultDto(true, "Password has been reset.");
    }
}
