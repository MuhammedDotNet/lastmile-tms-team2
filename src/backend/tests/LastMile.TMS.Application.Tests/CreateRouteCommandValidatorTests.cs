using FluentAssertions;
using LastMile.TMS.Application.Routes.Commands;
using LastMile.TMS.Application.Routes.DTOs;

namespace LastMile.TMS.Application.Tests;

public class CreateRouteCommandValidatorTests
{
    private readonly CreateRouteCommandValidator _validator = new();

    private static CreateRouteCommand ValidCommand() =>
        new(
            new CreateRouteDto
            {
                VehicleId = Guid.NewGuid(),
                DriverId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow,
                StartMileage = 0,
                ParcelIds = [Guid.NewGuid()],
            });

    [Fact]
    public void Validate_WhenParcelIdsEmpty_ShouldFail()
    {
        var cmd = ValidCommand() with
        {
            Dto = new CreateRouteDto
            {
                VehicleId = Guid.NewGuid(),
                DriverId = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow,
                StartMileage = 0,
                ParcelIds = [],
            },
        };

        var result = _validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Dto.ParcelIds");
    }

    [Fact]
    public void Validate_WhenParcelIdsHasItems_ShouldPassParcelIdsRule()
    {
        var cmd = ValidCommand();

        var result = _validator.Validate(cmd);

        result.Errors.Should().NotContain(e => e.PropertyName == "Dto.ParcelIds");
    }
}
