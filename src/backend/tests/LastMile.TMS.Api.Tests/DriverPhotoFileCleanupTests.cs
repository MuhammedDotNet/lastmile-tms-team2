using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Infrastructure.Services;
using LastMile.TMS.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LastMile.TMS.Api.Tests;

[Collection(ApiTestCollection.Name)]
public class DriverPhotoFileCleanupTests(CustomWebApplicationFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task DeleteOrphanDriverPhotosAsync_DeletesOnlyUnreferencedFiles()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var cleanup = scope.ServiceProvider.GetRequiredService<IDriverPhotoFileCleanup>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var webRoot = !string.IsNullOrEmpty(env.WebRootPath)
            ? env.WebRootPath
            : Path.Combine(env.ContentRootPath, "wwwroot");
        var driversDir = Path.Combine(webRoot, "uploads", "drivers");
        Directory.CreateDirectory(driversDir);

        var keptId = Guid.NewGuid().ToString("N");
        var orphanId = Guid.NewGuid().ToString("N");
        var keptPath = Path.Combine(driversDir, $"{keptId}.jpg");
        var orphanPath = Path.Combine(driversDir, $"{orphanId}.jpg");

        try
        {
            await File.WriteAllTextAsync(keptPath, "k");
            await File.WriteAllTextAsync(orphanPath, "o");

            var driver = await db.Drivers.FindAsync(DbSeeder.TestDriverId);
            driver.Should().NotBeNull();
            driver!.PhotoUrl = $"/uploads/drivers/{keptId}.jpg";
            await db.SaveChangesAsync();

            var deleted = await cleanup.DeleteOrphanDriverPhotosAsync();

            // Folder may already contain other orphan files from local dev; we assert our orphan is gone and kept file remains.
            deleted.Should().BeGreaterThanOrEqualTo(1);
            File.Exists(keptPath).Should().BeTrue();
            File.Exists(orphanPath).Should().BeFalse();
        }
        finally
        {
            TryDelete(keptPath);
            TryDelete(orphanPath);
        }
    }

    [Fact]
    public async Task DeleteOrphanDriverPhotosAsync_DeletesOnlyUnreferencedObjectStorageFiles()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var cleanup = scope.ServiceProvider.GetRequiredService<IDriverPhotoFileCleanup>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<InMemoryFileStorageService>();

        var keptId = $"{Guid.NewGuid():N}.jpg";
        var orphanId = $"{Guid.NewGuid():N}.jpg";

        await fileStorage.UploadAsync(
            $"drivers/{keptId}",
            new MemoryStream("keep"u8.ToArray(), writable: false),
            "image/jpeg");
        await fileStorage.UploadAsync(
            $"drivers/{orphanId}",
            new MemoryStream("orphan"u8.ToArray(), writable: false),
            "image/jpeg");

        var driver = await db.Drivers.FindAsync(DbSeeder.TestDriverId);
        driver.Should().NotBeNull();
        driver!.PhotoUrl = $"/api/drivers/photo/{keptId}";
        await db.SaveChangesAsync();

        var deleted = await cleanup.DeleteOrphanDriverPhotosAsync();
        var keys = await fileStorage.ListKeysAsync("drivers/");

        deleted.Should().Be(1);
        keys.Should().Contain($"drivers/{keptId}");
        keys.Should().NotContain($"drivers/{orphanId}");
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
