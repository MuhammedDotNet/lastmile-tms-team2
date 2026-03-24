using FluentAssertions;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Domain.Tests;

public class VehicleTests
{
    [Fact]
    public void Vehicle_ShouldInitializeWithDefaultValues()
    {
        var vehicle = new Vehicle();

        vehicle.RegistrationPlate.Should().Be(string.Empty);
        vehicle.Type.Should().Be(default(VehicleType));
        vehicle.ParcelCapacity.Should().Be(0);
        vehicle.WeightCapacity.Should().Be(0);
        vehicle.Status.Should().Be(default(VehicleStatus));
        vehicle.DepotId.Should().BeEmpty();
    }

    [Fact]
    public void Vehicle_ShouldHaveIdFromBaseEntity()
    {
        var vehicle = new Vehicle { Id = Guid.NewGuid() };

        vehicle.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Vehicle_ShouldAllowSettingAllProperties()
    {
        var depot = new Depot { Id = Guid.NewGuid(), Name = "Sydney Depot" };

        var vehicle = new Vehicle
        {
            RegistrationPlate = "ABC-123",
            Type = VehicleType.Van,
            ParcelCapacity = 50,
            WeightCapacity = 500.5m,
            Status = VehicleStatus.Available,
            DepotId = depot.Id,
            Depot = depot
        };

        vehicle.RegistrationPlate.Should().Be("ABC-123");
        vehicle.Type.Should().Be(VehicleType.Van);
        vehicle.ParcelCapacity.Should().Be(50);
        vehicle.WeightCapacity.Should().Be(500.5m);
        vehicle.Status.Should().Be(VehicleStatus.Available);
        vehicle.DepotId.Should().Be(depot.Id);
        vehicle.Depot.Should().Be(depot);
    }

    [Fact]
    public void Vehicle_ShouldSupportAllVehicleTypes()
    {
        var vehicle = new Vehicle();

        vehicle.Type = VehicleType.Van;
        vehicle.Type.Should().Be(VehicleType.Van);

        vehicle.Type = VehicleType.Car;
        vehicle.Type.Should().Be(VehicleType.Car);

        vehicle.Type = VehicleType.Bike;
        vehicle.Type.Should().Be(VehicleType.Bike);
    }

    [Fact]
    public void Vehicle_ShouldSupportAllVehicleStatuses()
    {
        var vehicle = new Vehicle();

        vehicle.Status = VehicleStatus.Available;
        vehicle.Status.Should().Be(VehicleStatus.Available);

        vehicle.Status = VehicleStatus.InUse;
        vehicle.Status.Should().Be(VehicleStatus.InUse);

        vehicle.Status = VehicleStatus.Maintenance;
        vehicle.Status.Should().Be(VehicleStatus.Maintenance);

        vehicle.Status = VehicleStatus.Retired;
        vehicle.Status.Should().Be(VehicleStatus.Retired);
    }
}