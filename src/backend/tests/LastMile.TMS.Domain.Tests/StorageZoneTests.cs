using FluentAssertions;
using LastMile.TMS.Domain.Entities;

namespace LastMile.TMS.Domain.Tests;

public class StorageZoneTests
{
    [Fact]
    public void StorageZone_ShouldInitializeWithDefaultValues()
    {
        var storageZone = new StorageZone();

        storageZone.Name.Should().Be(string.Empty);
        storageZone.NormalizedName.Should().Be(string.Empty);
        storageZone.StorageAisles.Should().BeEmpty();
        storageZone.CreatedBy.Should().BeNull();
        storageZone.LastModifiedBy.Should().BeNull();
        storageZone.LastModifiedAt.Should().BeNull();
    }

    [Fact]
    public void StorageZone_ShouldAllowSettingParentDepot()
    {
        var depot = new Depot { Id = Guid.NewGuid(), Name = "Test Depot" };
        var storageZone = new StorageZone
        {
            Name = "Receiving",
            NormalizedName = "RECEIVING",
            DepotId = depot.Id,
            Depot = depot
        };

        storageZone.DepotId.Should().Be(depot.Id);
        storageZone.Depot.Should().BeSameAs(depot);
        storageZone.Depot.Should().BeOfType<Depot>();
    }

    [Fact]
    public void StorageZone_ShouldSupportManagingStorageAisles()
    {
        var storageZone = new StorageZone { Name = "Storage" };
        var aisle = new StorageAisle { Name = "Aisle 1" };

        storageZone.StorageAisles.Add(aisle);

        storageZone.StorageAisles.Should().ContainSingle().Which.Should().BeSameAs(aisle);

        storageZone.StorageAisles.Remove(aisle);

        storageZone.StorageAisles.Should().BeEmpty();
    }
}
