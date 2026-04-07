using FluentAssertions;
using LastMile.TMS.Application.BinLocations.Commands;
using LastMile.TMS.Application.BinLocations.DTOs;

namespace LastMile.TMS.Application.Tests.BinLocations;

public class BinLocationCommandValidatorTests
{
    [Fact]
    public void CreateStorageZoneCommandValidator_ShouldRejectMissingNameAndDepotId()
    {
        var validator = new CreateStorageZoneCommandValidator();
        var command = new CreateStorageZoneCommand(new CreateStorageZoneDto
        {
            Name = string.Empty,
            DepotId = Guid.Empty
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.Name");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.DepotId");
    }

    [Fact]
    public void CreateStorageAisleCommandValidator_ShouldRejectMissingNameAndStorageZoneId()
    {
        var validator = new CreateStorageAisleCommandValidator();
        var command = new CreateStorageAisleCommand(new CreateStorageAisleDto
        {
            Name = string.Empty,
            StorageZoneId = Guid.Empty
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.Name");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.StorageZoneId");
    }

    [Fact]
    public void CreateBinLocationCommandValidator_ShouldRejectMissingNameAndStorageAisleId()
    {
        var validator = new CreateBinLocationCommandValidator();
        var command = new CreateBinLocationCommand(new CreateBinLocationDto
        {
            Name = string.Empty,
            StorageAisleId = Guid.Empty,
            IsActive = true
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.Name");
        result.Errors.Should().Contain(x => x.PropertyName == "Dto.StorageAisleId");
    }

    [Fact]
    public void UpdateBinLocationCommandValidator_ShouldRequireBinId()
    {
        var validator = new UpdateBinLocationCommandValidator();
        var command = new UpdateBinLocationCommand(Guid.Empty, new UpdateBinLocationDto
        {
            Name = "BIN-01",
            StorageAisleId = Guid.NewGuid(),
            IsActive = true
        });

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == nameof(UpdateBinLocationCommand.Id));
    }
}
