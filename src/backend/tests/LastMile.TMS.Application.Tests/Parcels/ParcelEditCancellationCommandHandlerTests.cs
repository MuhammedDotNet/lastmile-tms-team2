using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.Commands;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Parcels;

public class ParcelEditCancellationCommandHandlerTests
{
    private static readonly GeometryFactory GeoFactory = new(new PrecisionModel(), 4326);

    private static AppDbContext MakeDbContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    public static TheoryData<ParcelStatus> EditableStatuses => new()
    {
        ParcelStatus.Registered,
        ParcelStatus.ReceivedAtDepot,
        ParcelStatus.Sorted,
        ParcelStatus.Staged,
    };

    public static TheoryData<ParcelStatus> NonEditableStatuses => new()
    {
        ParcelStatus.Loaded,
        ParcelStatus.OutForDelivery,
    };

    [Theory]
    [MemberData(nameof(EditableStatuses))]
    public async Task UpdateParcel_AllowsEditingForPreLoadStatuses(ParcelStatus status)
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, status);
        var geocoding = Substitute.For<IGeocodingService>();
        var zoneMatching = Substitute.For<IZoneMatchingService>();
        var handler = MakeUpdateHandler(db, geocoding, zoneMatching);

        var command = new UpdateParcelCommand(MakeUpdateDto(
            fixture.Parcel,
            description: "Updated description"));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Description.Should().Be("Updated description");
        result.Status.Should().Be(status.ToString());
        result.ChangeHistory.Should().ContainSingle(entry =>
            entry.FieldName == "Description"
            && entry.BeforeValue == "Original description"
            && entry.AfterValue == "Updated description");

        geocoding.DidNotReceiveWithAnyArgs().GeocodeAsync(default!, default);
        zoneMatching.DidNotReceiveWithAnyArgs().FindZoneIdAsync(default!, default);
    }

    [Theory]
    [MemberData(nameof(NonEditableStatuses))]
    public async Task UpdateParcel_RejectsEditingAfterLoad(ParcelStatus status)
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, status);
        var geocoding = Substitute.For<IGeocodingService>();
        var zoneMatching = Substitute.For<IZoneMatchingService>();
        var handler = MakeUpdateHandler(db, geocoding, zoneMatching);

        var act = () => handler.Handle(
            new UpdateParcelCommand(MakeUpdateDto(fixture.Parcel, description: "Blocked")),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be edited*");
    }

    [Fact]
    public async Task UpdateParcel_AddressChange_RecalculatesZoneAndLogsDerivedChanges()
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, ParcelStatus.Registered);
        var geocoding = Substitute.For<IGeocodingService>();
        geocoding.GeocodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(MakePoint(31.40, 30.10));

        var zoneMatching = Substitute.For<IZoneMatchingService>();
        zoneMatching.FindZoneIdAsync(Arg.Any<Point>(), Arg.Any<CancellationToken>())
            .Returns(fixture.NextZone.Id);

        var handler = MakeUpdateHandler(db, geocoding, zoneMatching);
        var command = new UpdateParcelCommand(MakeUpdateDto(
            fixture.Parcel,
            shipperAddressId: fixture.NextShipperAddress.Id,
            street1: "250 Changed Street",
            city: "Giza"));

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result!.RecipientAddress.Street1.Should().Be("250 Changed Street");
        result.RecipientAddress.City.Should().Be("Giza");
        result.ZoneId.Should().Be(fixture.NextZone.Id);
        result.DepotName.Should().Be(fixture.NextDepot.Name);
        result.ChangeHistory.Should().Contain(entry => entry.FieldName == "Recipient street 1");
        result.ChangeHistory.Should().Contain(entry => entry.FieldName == "Sort zone");
        result.ChangeHistory.Should().Contain(entry => entry.FieldName == "Depot");
        result.ChangeHistory.Should().Contain(entry => entry.FieldName == "Sender depot");

        await geocoding.Received(1).GeocodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await zoneMatching.Received(1).FindZoneIdAsync(Arg.Any<Point>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateParcel_NoOpEdit_DoesNotCreateHistoryOrTouchLastModified()
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, ParcelStatus.Registered);
        var geocoding = Substitute.For<IGeocodingService>();
        var zoneMatching = Substitute.For<IZoneMatchingService>();
        var handler = MakeUpdateHandler(db, geocoding, zoneMatching);

        var result = await handler.Handle(
            new UpdateParcelCommand(MakeUpdateDto(fixture.Parcel)),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.ChangeHistory.Should().BeEmpty();

        var parcel = await db.Parcels
            .Include(p => p.RecipientAddress)
            .Include(p => p.ChangeHistory)
            .SingleAsync();

        parcel.LastModifiedAt.Should().BeNull();
        parcel.RecipientAddress.LastModifiedAt.Should().BeNull();
        parcel.ChangeHistory.Should().BeEmpty();

        geocoding.DidNotReceiveWithAnyArgs().GeocodeAsync(default!, default);
    }

    [Theory]
    [MemberData(nameof(EditableStatuses))]
    public async Task CancelParcel_AllowsCancellingForPreLoadStatuses(ParcelStatus status)
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, status);
        var handler = MakeCancelHandler(db);

        var result = await handler.Handle(
            new CancelParcelCommand(fixture.Parcel.Id, "Customer requested cancellation"),
            CancellationToken.None);

        result.Should().NotBeNull();
        result!.Status.Should().Be("Cancelled");
        result.CancellationReason.Should().Be("Customer requested cancellation");
        result.ChangeHistory.Should().Contain(entry =>
            entry.FieldName == "Status"
            && entry.BeforeValue == FormatEnum(status)
            && entry.AfterValue == "Cancelled");
        result.ChangeHistory.Should().Contain(entry =>
            entry.FieldName == "Cancellation reason"
            && entry.AfterValue == "Customer requested cancellation");

        var parcel = await db.Parcels
            .Include(p => p.RecipientAddress)
            .Include(p => p.ChangeHistory)
            .SingleAsync();

        parcel.Status.Should().Be(ParcelStatus.Cancelled);
        parcel.CancellationReason.Should().Be("Customer requested cancellation");
        parcel.LastModifiedAt.Should().NotBeNull();
        parcel.RecipientAddress.LastModifiedAt.Should().NotBeNull();
        parcel.ChangeHistory.Should().HaveCount(2);
    }

    [Fact]
    public async Task CancelParcel_RequiresReason()
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, ParcelStatus.Registered);
        var handler = MakeCancelHandler(db);

        var act = () => handler.Handle(
            new CancelParcelCommand(fixture.Parcel.Id, "   "),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*reason is required*");
    }

    [Theory]
    [MemberData(nameof(NonEditableStatuses))]
    public async Task CancelParcel_RejectsLoadedStatuses(ParcelStatus status)
    {
        using var db = MakeDbContext();
        var fixture = await SeedParcelAsync(db, status);
        var handler = MakeCancelHandler(db);

        var act = () => handler.Handle(
            new CancelParcelCommand(fixture.Parcel.Id, "Too late"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*cannot be cancelled*");
    }

    private static UpdateParcelCommandHandler MakeUpdateHandler(
        AppDbContext db,
        IGeocodingService geocoding,
        IZoneMatchingService zoneMatching)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserName.Returns("warehouse.user");

        return new UpdateParcelCommandHandler(db, geocoding, zoneMatching, currentUser);
    }

    private static CancelParcelCommandHandler MakeCancelHandler(AppDbContext db)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserName.Returns("warehouse.user");

        return new CancelParcelCommandHandler(db, currentUser);
    }

    private static async Task<ParcelFixture> SeedParcelAsync(AppDbContext db, ParcelStatus status)
    {
        var currentShipperAddress = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "10 Current Depot St",
            City = "Cairo",
            State = "Cairo",
            PostalCode = "11511",
            CountryCode = "EG",
        };

        var nextShipperAddress = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "20 Next Depot St",
            City = "Giza",
            State = "Giza",
            PostalCode = "12611",
            CountryCode = "EG",
        };

        var currentDepot = new Depot
        {
            Id = Guid.NewGuid(),
            Name = "Current Depot",
            Address = currentShipperAddress,
            AddressId = currentShipperAddress.Id,
            IsActive = true,
        };

        var nextDepot = new Depot
        {
            Id = Guid.NewGuid(),
            Name = "Next Depot",
            Address = nextShipperAddress,
            AddressId = nextShipperAddress.Id,
            IsActive = true,
        };

        var currentZone = new Zone
        {
            Id = Guid.NewGuid(),
            Name = "Current Zone",
            Boundary = MakePolygon(),
            Depot = currentDepot,
            DepotId = currentDepot.Id,
            IsActive = true,
        };

        var nextZone = new Zone
        {
            Id = Guid.NewGuid(),
            Name = "Next Zone",
            Boundary = MakePolygon(1),
            Depot = nextDepot,
            DepotId = nextDepot.Id,
            IsActive = true,
        };

        var recipientAddress = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "123 Existing Street",
            City = "Cairo",
            State = "Cairo",
            PostalCode = "11511",
            CountryCode = "EG",
            IsResidential = true,
            ContactName = "Jamie Carter",
            CompanyName = "Warehouse Co",
            Phone = "+201234567890",
            Email = "jamie@example.com",
            GeoLocation = MakePoint(31.23, 29.97),
        };

        var parcel = new Parcel
        {
            Id = Guid.NewGuid(),
            TrackingNumber = "LM202604020001",
            Description = "Original description",
            ServiceType = ServiceType.Standard,
            Status = status,
            ShipperAddress = currentShipperAddress,
            ShipperAddressId = currentShipperAddress.Id,
            RecipientAddress = recipientAddress,
            Weight = 2.5m,
            WeightUnit = WeightUnit.Kg,
            Length = 20m,
            Width = 15m,
            Height = 10m,
            DimensionUnit = DimensionUnit.Cm,
            DeclaredValue = 100m,
            Currency = "USD",
            EstimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(5),
            ParcelType = "Box",
            Zone = currentZone,
            ZoneId = currentZone.Id,
        };

        db.Addresses.AddRange(currentShipperAddress, nextShipperAddress, recipientAddress);
        db.Depots.AddRange(currentDepot, nextDepot);
        db.Zones.AddRange(currentZone, nextZone);
        db.Parcels.Add(parcel);
        await db.SaveChangesAsync();

        return new ParcelFixture(parcel, currentShipperAddress, nextShipperAddress, currentDepot, nextDepot, currentZone, nextZone);
    }

    private static UpdateParcelDto MakeUpdateDto(
        Parcel parcel,
        Guid? shipperAddressId = null,
        string? street1 = null,
        string? city = null,
        string? description = null) =>
        new()
        {
            Id = parcel.Id,
            ShipperAddressId = shipperAddressId ?? parcel.ShipperAddressId,
            RecipientAddress = new RegisterParcelRecipientAddressDto
            {
                Street1 = street1 ?? parcel.RecipientAddress.Street1,
                Street2 = parcel.RecipientAddress.Street2,
                City = city ?? parcel.RecipientAddress.City,
                State = parcel.RecipientAddress.State,
                PostalCode = parcel.RecipientAddress.PostalCode,
                CountryCode = parcel.RecipientAddress.CountryCode,
                IsResidential = parcel.RecipientAddress.IsResidential,
                ContactName = parcel.RecipientAddress.ContactName,
                CompanyName = parcel.RecipientAddress.CompanyName,
                Phone = parcel.RecipientAddress.Phone,
                Email = parcel.RecipientAddress.Email,
            },
            Description = description ?? parcel.Description,
            ServiceType = parcel.ServiceType,
            Weight = parcel.Weight,
            WeightUnit = parcel.WeightUnit,
            Length = parcel.Length,
            Width = parcel.Width,
            Height = parcel.Height,
            DimensionUnit = parcel.DimensionUnit,
            DeclaredValue = parcel.DeclaredValue,
            Currency = parcel.Currency,
            EstimatedDeliveryDate = parcel.EstimatedDeliveryDate.UtcDateTime,
            ParcelType = parcel.ParcelType,
        };

    private static Point MakePoint(double lon, double lat)
    {
        var point = GeoFactory.CreatePoint(new Coordinate(lon, lat));
        point.SRID = 4326;
        return point;
    }

    private static Polygon MakePolygon(double offset = 0)
    {
        var polygon = GeoFactory.CreatePolygon(
            [
                new Coordinate(31.20 + offset, 29.90 + offset),
                new Coordinate(31.30 + offset, 29.90 + offset),
                new Coordinate(31.30 + offset, 30.00 + offset),
                new Coordinate(31.20 + offset, 30.00 + offset),
                new Coordinate(31.20 + offset, 29.90 + offset),
            ]);
        polygon.SRID = 4326;
        return polygon;
    }

    private static string FormatEnum(Enum value)
    {
        var chars = value.ToString().ToCharArray();
        var builder = new System.Text.StringBuilder();
        for (var index = 0; index < chars.Length; index++)
        {
            var current = chars[index];
            if (index > 0 && char.IsUpper(current))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    private sealed record ParcelFixture(
        Parcel Parcel,
        Address CurrentShipperAddress,
        Address NextShipperAddress,
        Depot CurrentDepot,
        Depot NextDepot,
        Zone CurrentZone,
        Zone NextZone);
}
