using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Parcels.Commands;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Domain.Entities;
using LastMile.TMS.Domain.Enums;
using LastMile.TMS.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Parcels;

public class InboundReceivingCommandHandlerTests
{
    private static AppDbContext MakeDbContext() =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

    [Fact]
    public async Task StartInboundReceivingSession_ReturnsExistingOpenSessionForManifest()
    {
        await using var db = MakeDbContext();
        var fixture = await SeedFixtureAsync(db);
        var currentUser = CreateCurrentUser(fixture.Operator);
        var handler = new StartInboundReceivingSessionCommandHandler(db, currentUser);

        var first = await handler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        var second = await handler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        first.Id.Should().Be(second.Id);
        db.InboundReceivingSessions.Should().HaveCount(1);
    }

    [Fact]
    public async Task ScanInboundParcel_ExpectedParcel_UpdatesStatusAndTracking()
    {
        await using var db = MakeDbContext();
        var fixture = await SeedFixtureAsync(db);
        var currentUser = CreateCurrentUser(fixture.Operator);
        var startHandler = new StartInboundReceivingSessionCommandHandler(db, currentUser);
        var notifier = Substitute.For<IParcelUpdateNotifier>();
        var scanHandler = new ScanInboundParcelCommandHandler(db, currentUser, notifier);

        var session = await startHandler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        var result = await scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.ExpectedParcel.TrackingNumber),
            CancellationToken.None);

        result.IsExpected.Should().BeTrue();
        result.ScannedParcel.TrackingNumber.Should().Be(fixture.ExpectedParcel.TrackingNumber);
        result.ScannedParcel.MatchType.Should().Be("Expected");

        var parcel = await db.Parcels
            .Include(p => p.TrackingEvents)
            .SingleAsync(p => p.Id == fixture.ExpectedParcel.Id);

        parcel.Status.Should().Be(ParcelStatus.ReceivedAtDepot);
        parcel.TrackingEvents.Should().ContainSingle();
        parcel.TrackingEvents.Single().Location.Should().Be(fixture.Depot.Name);
        parcel.TrackingEvents.Single().Description.Should().Contain("inbound");

        db.InboundReceivingExceptions.Should().BeEmpty();
    }

    [Fact]
    public async Task ScanInboundParcel_RegisteredParcelOutsideManifest_AddsUnexpectedException()
    {
        await using var db = MakeDbContext();
        var fixture = await SeedFixtureAsync(db);
        var currentUser = CreateCurrentUser(fixture.Operator);
        var startHandler = new StartInboundReceivingSessionCommandHandler(db, currentUser);
        var notifier = Substitute.For<IParcelUpdateNotifier>();
        var scanHandler = new ScanInboundParcelCommandHandler(db, currentUser, notifier);

        var session = await startHandler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        var result = await scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.UnexpectedParcel.TrackingNumber),
            CancellationToken.None);

        result.IsExpected.Should().BeFalse();
        result.ScannedParcel.MatchType.Should().Be("Unexpected");

        var exception = await db.InboundReceivingExceptions.SingleAsync();
        exception.ExceptionType.Should().Be(InboundReceivingExceptionType.Unexpected);
        exception.ParcelId.Should().Be(fixture.UnexpectedParcel.Id);
    }

    [Fact]
    public async Task ScanInboundParcel_RejectsUnknownDuplicateCrossDepotAndAlreadyReceivedBarcodes()
    {
        await using var db = MakeDbContext();
        var fixture = await SeedFixtureAsync(db);
        var currentUser = CreateCurrentUser(fixture.Operator);
        var startHandler = new StartInboundReceivingSessionCommandHandler(db, currentUser);
        var notifier = Substitute.For<IParcelUpdateNotifier>();
        var scanHandler = new ScanInboundParcelCommandHandler(db, currentUser, notifier);

        var session = await startHandler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        var unknown = () => scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, "UNKNOWN-BARCODE"),
            CancellationToken.None);

        await unknown.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not registered*");

        await scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.ExpectedParcel.TrackingNumber),
            CancellationToken.None);

        var duplicate = () => scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.ExpectedParcel.TrackingNumber),
            CancellationToken.None);

        await duplicate.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been scanned*");

        var crossDepot = () => scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.CrossDepotParcel.TrackingNumber),
            CancellationToken.None);

        await crossDepot.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*different depot*");

        var alreadyReceived = () => scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.AlreadyReceivedParcel.TrackingNumber),
            CancellationToken.None);

        await alreadyReceived.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been received*");
    }

    [Fact]
    public async Task ConfirmInboundReceivingSession_CreatesMissingExceptionsAndClosesManifestAndSession()
    {
        await using var db = MakeDbContext();
        var fixture = await SeedFixtureAsync(db);
        var currentUser = CreateCurrentUser(fixture.Operator);
        var startHandler = new StartInboundReceivingSessionCommandHandler(db, currentUser);
        var notifier = Substitute.For<IParcelUpdateNotifier>();
        var scanHandler = new ScanInboundParcelCommandHandler(db, currentUser, notifier);
        var confirmHandler = new ConfirmInboundReceivingSessionCommandHandler(db, currentUser);

        var session = await startHandler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        await scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.ExpectedParcel.TrackingNumber),
            CancellationToken.None);

        await scanHandler.Handle(
            new ScanInboundParcelCommand(session.Id, fixture.UnexpectedParcel.TrackingNumber),
            CancellationToken.None);

        var confirmed = await confirmHandler.Handle(
            new ConfirmInboundReceivingSessionCommand(session.Id),
            CancellationToken.None);

        confirmed.Status.Should().Be("Confirmed");
        confirmed.Exceptions.Should().ContainSingle(e =>
            e.ExceptionType == "Missing" && e.TrackingNumber == fixture.MissingExpectedParcel.TrackingNumber);
        confirmed.Exceptions.Should().ContainSingle(e =>
            e.ExceptionType == "Unexpected" && e.TrackingNumber == fixture.UnexpectedParcel.TrackingNumber);

        var persistedSession = await db.InboundReceivingSessions.SingleAsync();
        var persistedManifest = await db.InboundManifests.SingleAsync();

        persistedSession.Status.Should().Be(InboundReceivingSessionStatus.Confirmed);
        persistedManifest.Status.Should().Be(InboundManifestStatus.Closed);
    }

    [Fact]
    public async Task ConfirmInboundReceivingSession_RejectsAlreadyConfirmedSession()
    {
        await using var db = MakeDbContext();
        var fixture = await SeedFixtureAsync(db);
        var currentUser = CreateCurrentUser(fixture.Operator);
        var startHandler = new StartInboundReceivingSessionCommandHandler(db, currentUser);
        var confirmHandler = new ConfirmInboundReceivingSessionCommandHandler(db, currentUser);

        var session = await startHandler.Handle(
            new StartInboundReceivingSessionCommand(fixture.Manifest.Id),
            CancellationToken.None);

        await confirmHandler.Handle(
            new ConfirmInboundReceivingSessionCommand(session.Id),
            CancellationToken.None);

        var act = () => confirmHandler.Handle(
            new ConfirmInboundReceivingSessionCommand(session.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already been confirmed*");
    }

    private static ICurrentUserService CreateCurrentUser(ApplicationUser user)
    {
        var currentUser = Substitute.For<ICurrentUserService>();
        currentUser.UserId.Returns(user.Id.ToString());
        currentUser.UserName.Returns(user.UserName);
        currentUser.Roles.Returns(["WarehouseOperator"]);
        return currentUser;
    }

    private static async Task<InboundFixture> SeedFixtureAsync(AppDbContext db)
    {
        var depotAddress = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "1 Depot Street",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2000",
            CountryCode = "AU",
        };

        var depot = new Depot
        {
            Id = Guid.NewGuid(),
            Name = "Sydney Inbound Depot",
            AddressId = depotAddress.Id,
            Address = depotAddress,
            IsActive = true,
        };

        var zone = new Zone
        {
            Id = Guid.NewGuid(),
            Name = "Sydney Inbound Zone",
            Boundary = TestsPolygonFactory.CreateDefault(),
            DepotId = depot.Id,
            Depot = depot,
            IsActive = true,
        };

        var otherDepotAddress = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "2 Other Depot Street",
            City = "Melbourne",
            State = "VIC",
            PostalCode = "3000",
            CountryCode = "AU",
        };

        var otherDepot = new Depot
        {
            Id = Guid.NewGuid(),
            Name = "Melbourne Depot",
            AddressId = otherDepotAddress.Id,
            Address = otherDepotAddress,
            IsActive = true,
        };

        var otherZone = new Zone
        {
            Id = Guid.NewGuid(),
            Name = "Melbourne Zone",
            Boundary = TestsPolygonFactory.CreateOffset(1),
            DepotId = otherDepot.Id,
            Depot = otherDepot,
            IsActive = true,
        };

        var shipper = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "10 Shipper Lane",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2001",
            CountryCode = "AU",
        };

        var recipient = new Address
        {
            Id = Guid.NewGuid(),
            Street1 = "20 Recipient Road",
            City = "Sydney",
            State = "NSW",
            PostalCode = "2002",
            CountryCode = "AU",
            ContactName = "Recipient",
        };

        var operatorUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "warehouse.operator@example.com",
            Email = "warehouse.operator@example.com",
            FirstName = "Warehouse",
            LastName = "Operator",
            IsActive = true,
            DepotId = depot.Id,
            ZoneId = zone.Id,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var expectedParcel = CreateParcel("LMINBOUND0001", ParcelStatus.Registered, shipper, recipient, zone);
        var missingExpectedParcel = CreateParcel("LMINBOUND0002", ParcelStatus.Registered, shipper, recipient, zone);
        var unexpectedParcel = CreateParcel("LMINBOUND0099", ParcelStatus.Registered, shipper, recipient, zone);
        var crossDepotParcel = CreateParcel("LMCROSSDEPOT01", ParcelStatus.Registered, shipper, recipient, otherZone);
        var alreadyReceivedParcel = CreateParcel("LMRECEIVED0001", ParcelStatus.ReceivedAtDepot, shipper, recipient, zone);

        var manifest = new InboundManifest
        {
            Id = Guid.NewGuid(),
            ManifestNumber = "MAN-1001",
            TruckIdentifier = "TRUCK-7",
            DepotId = depot.Id,
            Depot = depot,
            Status = InboundManifestStatus.Open,
            Lines =
            [
                new InboundManifestLine
                {
                    Id = Guid.NewGuid(),
                    ParcelId = expectedParcel.Id,
                    Parcel = expectedParcel,
                },
                new InboundManifestLine
                {
                    Id = Guid.NewGuid(),
                    ParcelId = missingExpectedParcel.Id,
                    Parcel = missingExpectedParcel,
                },
            ],
        };

        db.AddRange(
            depotAddress,
            depot,
            zone,
            otherDepotAddress,
            otherDepot,
            otherZone,
            shipper,
            recipient,
            operatorUser,
            expectedParcel,
            missingExpectedParcel,
            unexpectedParcel,
            crossDepotParcel,
            alreadyReceivedParcel,
            manifest);

        await db.SaveChangesAsync();

        return new InboundFixture(
            depot,
            operatorUser,
            manifest,
            expectedParcel,
            missingExpectedParcel,
            unexpectedParcel,
            crossDepotParcel,
            alreadyReceivedParcel);
    }

    private static Parcel CreateParcel(
        string trackingNumber,
        ParcelStatus status,
        Address shipper,
        Address recipient,
        Zone zone) =>
        new()
        {
            Id = Guid.NewGuid(),
            TrackingNumber = trackingNumber,
            Description = "Inbound test parcel",
            ServiceType = ServiceType.Standard,
            Status = status,
            ShipperAddressId = shipper.Id,
            ShipperAddress = shipper,
            RecipientAddressId = recipient.Id,
            RecipientAddress = recipient,
            Weight = 1.2m,
            WeightUnit = WeightUnit.Kg,
            Length = 20,
            Width = 10,
            Height = 5,
            DimensionUnit = DimensionUnit.Cm,
            DeclaredValue = 50m,
            Currency = "AUD",
            EstimatedDeliveryDate = DateTimeOffset.UtcNow.AddDays(2),
            ZoneId = zone.Id,
            Zone = zone,
        };

    private sealed record InboundFixture(
        Depot Depot,
        ApplicationUser Operator,
        InboundManifest Manifest,
        Parcel ExpectedParcel,
        Parcel MissingExpectedParcel,
        Parcel UnexpectedParcel,
        Parcel CrossDepotParcel,
        Parcel AlreadyReceivedParcel);
}

internal static class TestsPolygonFactory
{
    private static readonly NetTopologySuite.Geometries.GeometryFactory GeoFactory =
        new(new NetTopologySuite.Geometries.PrecisionModel(), 4326);

    public static NetTopologySuite.Geometries.Polygon CreateDefault() => CreateOffset(0);

    public static NetTopologySuite.Geometries.Polygon CreateOffset(double offset)
    {
        var polygon = GeoFactory.CreatePolygon(
        [
            new NetTopologySuite.Geometries.Coordinate(151.0 + offset, -33.0 + offset),
            new NetTopologySuite.Geometries.Coordinate(152.0 + offset, -33.0 + offset),
            new NetTopologySuite.Geometries.Coordinate(152.0 + offset, -34.0 + offset),
            new NetTopologySuite.Geometries.Coordinate(151.0 + offset, -34.0 + offset),
            new NetTopologySuite.Geometries.Coordinate(151.0 + offset, -33.0 + offset),
        ]);
        polygon.SRID = 4326;
        return polygon;
    }
}
