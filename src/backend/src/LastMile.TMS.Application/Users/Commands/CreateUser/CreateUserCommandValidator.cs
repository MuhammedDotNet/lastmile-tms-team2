using FluentValidation;

namespace LastMile.TMS.Application.Users.Commands;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Dto.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Dto.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Dto.Phone).MaximumLength(20);
    }
}
