using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IoPath = System.IO.Path;

namespace LastMile.TMS.Api.Controllers;

/// <summary>
/// Stores driver profile photos under wwwroot and returns a URL path for <c>Driver.PhotoUrl</c>.
/// </summary>
[ApiController]
[Route("api/drivers")]
[Authorize(Roles = "Admin,OperationsManager")]
public sealed class DriverPhotosController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions =
    [
        ".jpg", ".jpeg", ".png", ".webp", ".gif",
    ];

    [HttpPost("photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(IFormFile? file, [FromServices] IWebHostEnvironment env)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var ext = IoPath.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            return BadRequest(new { message = "Allowed types: JPG, PNG, WebP, GIF." });

        var webRoot = env.WebRootPath;
        if (string.IsNullOrEmpty(webRoot))
            webRoot = IoPath.Combine(env.ContentRootPath, "wwwroot");

        var dir = IoPath.Combine(webRoot, "uploads", "drivers");
        Directory.CreateDirectory(dir);

        var name = $"{Guid.NewGuid():N}{ext}";
        var physical = IoPath.Combine(dir, name);

        await using (var stream = System.IO.File.Create(physical))
        {
            await file.CopyToAsync(stream);
        }

        var relative = $"/uploads/drivers/{name}";
        return Ok(new { url = relative });
    }
}
