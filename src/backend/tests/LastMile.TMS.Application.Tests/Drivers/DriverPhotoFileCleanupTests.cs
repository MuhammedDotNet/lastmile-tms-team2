using FluentAssertions;
using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;

namespace LastMile.TMS.Application.Tests.Drivers;

public class DriverPhotoFileCleanupTests
{
    [Fact]
    public async Task TryDeleteStoredPhotoAsync_RelativePath_DeletesMatchingFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"lm-driver-photo-{Guid.NewGuid():N}");
        try
        {
            var wwwroot = Path.Combine(root, "wwwroot");
            var driversDir = Path.Combine(wwwroot, "uploads", "drivers");
            Directory.CreateDirectory(driversDir);

            var name = $"{Guid.NewGuid():N}.jpg";
            var full = Path.Combine(driversDir, name);
            File.WriteAllText(full, "x");

            var env = Substitute.For<IWebHostEnvironment>();
            env.WebRootPath.Returns(wwwroot);
            var db = Substitute.For<IAppDbContext>();
            var fileStorage = new InMemoryFileStorageService();

            var sut = new DriverPhotoFileCleanup(env, db, fileStorage);
            await sut.TryDeleteStoredPhotoAsync($"/uploads/drivers/{name}");

            File.Exists(full).Should().BeFalse();
        }
        finally
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
                // best-effort temp cleanup
            }
        }
    }

    [Fact]
    public async Task TryDeleteStoredPhotoAsync_AbsoluteUrl_DeletesMatchingFile()
    {
        var root = Path.Combine(Path.GetTempPath(), $"lm-driver-photo-{Guid.NewGuid():N}");
        try
        {
            var wwwroot = Path.Combine(root, "wwwroot");
            var driversDir = Path.Combine(wwwroot, "uploads", "drivers");
            Directory.CreateDirectory(driversDir);

            var name = $"{Guid.NewGuid():N}.png";
            var full = Path.Combine(driversDir, name);
            File.WriteAllText(full, "x");

            var env = Substitute.For<IWebHostEnvironment>();
            env.WebRootPath.Returns(wwwroot);
            var db = Substitute.For<IAppDbContext>();
            var fileStorage = new InMemoryFileStorageService();

            var sut = new DriverPhotoFileCleanup(env, db, fileStorage);
            await sut.TryDeleteStoredPhotoAsync($"https://api.example.com/uploads/drivers/{name}");

            File.Exists(full).Should().BeFalse();
        }
        finally
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
            }
        }
    }

    [Fact]
    public async Task TryDeleteStoredPhotoAsync_InvalidFileName_DoesNotDeleteOutsideFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), $"lm-driver-photo-{Guid.NewGuid():N}");
        try
        {
            var wwwroot = Path.Combine(root, "wwwroot");
            Directory.CreateDirectory(wwwroot);
            var secret = Path.Combine(wwwroot, "secret.txt");
            File.WriteAllText(secret, "nope");

            var env = Substitute.For<IWebHostEnvironment>();
            env.WebRootPath.Returns(wwwroot);
            var db = Substitute.For<IAppDbContext>();
            var fileStorage = new InMemoryFileStorageService();

            var sut = new DriverPhotoFileCleanup(env, db, fileStorage);
            await sut.TryDeleteStoredPhotoAsync("/uploads/drivers/../../../secret.txt");

            File.Exists(secret).Should().BeTrue();
        }
        finally
        {
            try
            {
                if (Directory.Exists(root))
                    Directory.Delete(root, recursive: true);
            }
            catch
            {
            }
        }
    }
}
