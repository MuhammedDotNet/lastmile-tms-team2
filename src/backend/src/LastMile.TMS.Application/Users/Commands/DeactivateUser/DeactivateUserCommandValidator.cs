using FluentValidation;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class DeactivateUserCommandValidator : AbstractValidator<DeactivateUserCommand>
{
    public DeactivateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
