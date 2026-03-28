using FluentValidation;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class SendPasswordResetEmailCommandValidator : AbstractValidator<SendPasswordResetEmailCommand>
{
    public SendPasswordResetEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
