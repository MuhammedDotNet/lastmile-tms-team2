using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class RequestPasswordResetCommandHandler(
    UserManager<ApplicationUser> userManager,
    IUserAccountEmailJobScheduler emailJobScheduler)
    : IRequestHandler<RequestPasswordResetCommand, UserActionResultDto>
{
    public async Task<UserActionResultDto> Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);

        if (user is not null && user.IsActive)
        {
            await emailJobScheduler.SchedulePasswordResetEmailAsync(user.Id, cancellationToken);
        }

        return new UserActionResultDto(
            true,
            "If the email exists, a reset link has been sent.");
    }
}
