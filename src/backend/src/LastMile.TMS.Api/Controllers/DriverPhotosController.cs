using LastMile.TMS.Application.Common.Interfaces;
using LastMile.TMS.Application.Drivers.Support;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace LastMile.TMS.Api.Controllers;

/// <summary>
/// Stores driver profile photos in object storage and returns a proxy URL for <c>Driver.PhotoUrl</c>.
/// </summary>
[ApiController]
[Route("api/drivers")]
[Authorize(Roles = "Admin,OperationsManager")]
public sealed class DriverPhotosController : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    [HttpPost("photo")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPhoto(
        IFormFile? file,
        [FromServices] IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!DriverPhotoReference.IsAllowedExtension(ext))
            return BadRequest(new { message = "Allowed types: JPG, PNG, WebP, GIF." });

        var fileName = DriverPhotoReference.CreateFileName(ext);
        var key = DriverPhotoReference.BuildStorageKey(fileName);

        await using var stream = file.OpenReadStream();
        await fileStorageService.UploadAsync(
            key,
            stream,
            ResolveContentType(fileName, file.ContentType),
            cancellationToken);

        return Ok(new { url = DriverPhotoReference.BuildObjectStorageUrl(fileName) });
    }

    [AllowAnonymous]
    [HttpGet("photo/{fileName}")]
    public async Task<IActionResult> GetPhoto(
        string fileName,
        [FromServices] IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        if (!DriverPhotoReference.IsValidFileName(fileName))
        {
            return NotFound();
        }

        try
        {
            var storedObject = await fileStorageService.OpenReadAsync(
                DriverPhotoReference.BuildStorageKey(fileName),
                cancellationToken);

            var contentType = ResolveContentType(fileName, storedObject.ContentType);
            return File(storedObject.Content, contentType);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    private static string ResolveContentType(string fileName, string? contentType)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return contentType;
        }

        return ContentTypeProvider.TryGetContentType(fileName, out var resolvedContentType)
            ? resolvedContentType
            : "application/octet-stream";
    }
}
