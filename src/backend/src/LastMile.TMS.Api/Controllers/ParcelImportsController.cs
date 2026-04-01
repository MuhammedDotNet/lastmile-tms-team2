using System.Text;
using LastMile.TMS.Application.Parcels.Commands;
using LastMile.TMS.Application.Parcels.DTOs;
using LastMile.TMS.Application.Parcels.Queries;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LastMile.TMS.Api.Controllers;

[ApiController]
[Route("api/parcel-imports")]
[Authorize(Roles = "OperationsManager,Admin,Dispatcher,WarehouseOperator")]
public sealed class ParcelImportsController(
    ISender mediator,
    IParcelImportTemplateGenerator templateGenerator) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        [FromForm] UploadParcelImportRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            return BadRequest("A file is required.");
        }

        if (request.File.Length == 0)
        {
            return BadRequest("The uploaded file is empty.");
        }

        ParcelImportFileFormat fileFormat;
        try
        {
            fileFormat = GetFileFormat(request.File.FileName);
        }
        catch (ParcelImportFileValidationException ex)
        {
            return BadRequest(ex.Message);
        }

        await using var stream = request.File.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);

        try
        {
            var importId = await mediator.Send(
                new CreateParcelImportCommand(
                    new CreateParcelImportDto
                    {
                        ShipperAddressId = request.ShipperAddressId,
                        FileName = request.File.FileName,
                        FileFormat = fileFormat,
                        SourceFile = memoryStream.ToArray(),
                    }),
                cancellationToken);

            return Accepted(new { importId });
        }
        catch (ParcelImportFileValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("template.csv")]
    public IActionResult DownloadCsvTemplate()
    {
        var bytes = templateGenerator.GenerateTemplate(ParcelImportFileFormat.Csv);
        return File(bytes, "text/csv", "parcel-import-template.csv");
    }

    [HttpGet("template.xlsx")]
    public IActionResult DownloadXlsxTemplate()
    {
        var bytes = templateGenerator.GenerateTemplate(ParcelImportFileFormat.Xlsx);
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "parcel-import-template.xlsx");
    }

    [HttpGet("{id:guid}/errors.csv")]
    public async Task<IActionResult> DownloadErrorsCsv(Guid id, CancellationToken cancellationToken)
    {
        var report = await mediator.Send(new GetParcelImportErrorReportQuery(id), cancellationToken);
        if (report is null)
        {
            return NotFound();
        }

        var bytes = Encoding.UTF8.GetBytes(BuildErrorsCsv(report));
        var fileName = $"{System.IO.Path.GetFileNameWithoutExtension(report.FileName)}-errors.csv";
        return File(bytes, "text/csv", fileName);
    }

    private static ParcelImportFileFormat GetFileFormat(string fileName)
    {
        return System.IO.Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".csv" => ParcelImportFileFormat.Csv,
            ".xlsx" => ParcelImportFileFormat.Xlsx,
            _ => throw new ParcelImportFileValidationException("Only .csv and .xlsx files are supported."),
        };
    }

    private static string BuildErrorsCsv(ParcelImportErrorReportDto report)
    {
        var builder = new StringBuilder();
        builder.Append("row_number,error_message");
        foreach (var header in ParcelImportTemplateColumns.All)
        {
            builder.Append(',');
            builder.Append(header);
        }

        builder.AppendLine();

        foreach (var row in report.Rows)
        {
            builder.Append(row.RowNumber.ToString());
            builder.Append(',');
            builder.Append(EscapeCsv(row.ErrorMessage));

            foreach (var header in ParcelImportTemplateColumns.All)
            {
                builder.Append(',');
                row.Values.TryGetValue(header, out var value);
                builder.Append(EscapeCsv(value ?? string.Empty));
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}

public sealed class UploadParcelImportRequest
{
    public Guid ShipperAddressId { get; set; }
    public IFormFile? File { get; set; }
}
