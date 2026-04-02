using System.Text;
using ClosedXML.Excel;
using LastMile.TMS.Application.Parcels.Services;
using LastMile.TMS.Application.Parcels.Support;
using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Infrastructure.Services;

public sealed class ParcelImportTemplateGenerator : IParcelImportTemplateGenerator
{
    private static readonly string[] ExampleRow =
    [
        "15 George Street",
        "",
        "Sydney",
        "NSW",
        "2000",
        "AU",
        "true",
        "Taylor Smith",
        "Acme",
        "+61000000000",
        "taylor@example.com",
        "Box",
        "Package",
        "STANDARD",
        "2.5",
        "KG",
        "20",
        "10",
        "5",
        "CM",
        "100",
        "AUD",
        "2030-01-15",
    ];

    public byte[] GenerateTemplate(ParcelImportFileFormat format) =>
        format switch
        {
            ParcelImportFileFormat.Csv => GenerateCsvTemplate(),
            ParcelImportFileFormat.Xlsx => GenerateXlsxTemplate(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

    private static byte[] GenerateCsvTemplate()
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(",", ParcelImportTemplateColumns.All));
        builder.AppendLine(string.Join(",", ExampleRow.Select(EscapeCsv)));
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static byte[] GenerateXlsxTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Parcel Import");

        for (var columnIndex = 0; columnIndex < ParcelImportTemplateColumns.All.Length; columnIndex++)
        {
            worksheet.Cell(1, columnIndex + 1).Value = ParcelImportTemplateColumns.All[columnIndex];
            worksheet.Cell(2, columnIndex + 1).Value = ExampleRow[columnIndex];
        }

        worksheet.Row(1).Style.Font.Bold = true;
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
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
