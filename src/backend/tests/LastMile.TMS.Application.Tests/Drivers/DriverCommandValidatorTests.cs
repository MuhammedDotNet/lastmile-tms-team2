using FluentAssertions;
using LastMile.TMS.Application.Drivers.Commands;
using LastMile.TMS.Application.Drivers.DTOs;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Tests.Drivers;

public class DriverCommandValidatorTests
{
    [Fact]
    public void CreateDriverCommandValidator_RejectsDuplicateDayOfWeek()
    {
        var validator = new CreateDriverCommandValidator();
        var command = new CreateDriverCommand(new CreateDriverDto
        {
            FirstName = "A",
            LastName = "B",
            LicenseNumber = "LIC-1",
            ZoneId = Guid.NewGuid(),
            DepotId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AvailabilitySchedule =
            [
                new CreateDriverAvailabilityDto
                {
                    DayOfWeek = DayOfWeek.Monday,
                    IsAvailable = true,
                },
                new CreateDriverAvailabilityDto
                {
                    DayOfWeek = DayOfWeek.Monday,
                    IsAvailable = false,
                },
            ],
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto.AvailabilitySchedule");
    }

    [Fact]
    public void CreateDriverCommandValidator_AllowsPastLicenseExpiry()
    {
        var validator = new CreateDriverCommandValidator();
        var command = new CreateDriverCommand(new CreateDriverDto
        {
            FirstName = "A",
            LastName = "B",
            LicenseNumber = "LIC-2",
            ZoneId = Guid.NewGuid(),
            DepotId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            LicenseExpiryDate = DateTimeOffset.UtcNow.AddYears(-1),
            AvailabilitySchedule = [],
        });

        var result = validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName.Contains("LicenseExpiry", StringComparison.Ordinal));
    }

    [Fact]
    public void UpdateDriverCommandValidator_RejectsDuplicateDayOfWeek()
    {
        var validator = new UpdateDriverCommandValidator();
        var command = new UpdateDriverCommand(Guid.NewGuid(), new UpdateDriverDto
        {
            FirstName = "A",
            LastName = "B",
            LicenseNumber = "LIC-1",
            ZoneId = Guid.NewGuid(),
            DepotId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AvailabilitySchedule =
            [
                new UpdateDriverAvailabilityDto { DayOfWeek = DayOfWeek.Tuesday, IsAvailable = true },
                new UpdateDriverAvailabilityDto { DayOfWeek = DayOfWeek.Tuesday, IsAvailable = false },
            ],
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto.AvailabilitySchedule");
    }
}
