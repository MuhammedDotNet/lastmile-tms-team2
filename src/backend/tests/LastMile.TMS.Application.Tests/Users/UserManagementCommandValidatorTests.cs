using FluentAssertions;
using LastMile.TMS.Application.Users.Commands;
using LastMile.TMS.Application.Users.DTOs;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Tests.Users;

public class UserManagementCommandValidatorTests
{
    [Fact]
    public void CreateUserCommandValidator_ShouldRejectMissingRequiredFields()
    {
        var validator = new CreateUserCommandValidator();
        var command = new CreateUserCommand(new CreateUserDto
        {
            FirstName = "",
            LastName = "",
            Email = "not-an-email",
            Phone = new string('1', 25),
            Role = PredefinedRole.Dispatcher
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.FirstName");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.LastName");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.Email");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.Phone");
    }

    [Fact]
    public void UpdateUserCommandValidator_ShouldRequireUserId()
    {
        var validator = new UpdateUserCommandValidator();
        var command = new UpdateUserCommand(Guid.Empty, new UpdateUserDto
        {
            FirstName = "Taylor",
            LastName = "Updater",
            Email = "taylor@example.com",
            Phone = "+10000000003",
            Role = PredefinedRole.OperationsManager,
            IsActive = true
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateUserCommand.Id));
    }

    [Fact]
    public void DeactivateUserCommandValidator_ShouldRequireUserId()
    {
        var validator = new DeactivateUserCommandValidator();
        var command = new DeactivateUserCommand(Guid.Empty);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(DeactivateUserCommand.UserId));
    }

    [Fact]
    public void RequestPasswordResetCommandValidator_ShouldRequireValidEmail()
    {
        var validator = new RequestPasswordResetCommandValidator();
        var command = new RequestPasswordResetCommand("not-an-email");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(RequestPasswordResetCommand.Email));
    }

    [Fact]
    public void CompletePasswordResetCommandValidator_ShouldRejectWeakPasswords()
    {
        var validator = new CompletePasswordResetCommandValidator();
        var command = new CompletePasswordResetCommand(
            "person@example.com",
            "token-value",
            "weakpass");

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(CompletePasswordResetCommand.NewPassword));
    }

    [Fact]
    public void CompletePasswordResetCommandValidator_ShouldAcceptValidPasswords()
    {
        var validator = new CompletePasswordResetCommandValidator();
        var command = new CompletePasswordResetCommand(
            "person@example.com",
            "token-value",
            "ValidPass1");

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
