using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelImports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ParcelImportId",
                table: "Parcels",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ParcelImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    FileFormat = table.Column<string>(type: "text", nullable: false),
                    ShipperAddressId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SourceFile = table.Column<byte[]>(type: "bytea", nullable: false),
                    TotalRows = table.Column<int>(type: "integer", nullable: false),
                    ProcessedRows = table.Column<int>(type: "integer", nullable: false),
                    ImportedRows = table.Column<int>(type: "integer", nullable: false),
                    RejectedRows = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FailureMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelImports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelImports_Addresses_ShipperAddressId",
                        column: x => x.ShipperAddressId,
                        principalTable: "Addresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ParcelImportRowFailures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelImportId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowNumber = table.Column<int>(type: "integer", nullable: false),
                    OriginalRowValues = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParcelImportRowFailures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParcelImportRowFailures_ParcelImports_ParcelImportId",
                        column: x => x.ParcelImportId,
                        principalTable: "ParcelImports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parcels_ParcelImportId",
                table: "Parcels",
                column: "ParcelImportId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelImportRowFailures_ParcelImportId_RowNumber",
                table: "ParcelImportRowFailures",
                columns: new[] { "ParcelImportId", "RowNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ParcelImports_CreatedAt",
                table: "ParcelImports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelImports_ShipperAddressId",
                table: "ParcelImports",
                column: "ShipperAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ParcelImports_Status",
                table: "ParcelImports",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Parcels_ParcelImports_ParcelImportId",
                table: "Parcels",
                column: "ParcelImportId",
                principalTable: "ParcelImports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parcels_ParcelImports_ParcelImportId",
                table: "Parcels");

            migrationBuilder.DropTable(
                name: "ParcelImportRowFailures");

            migrationBuilder.DropTable(
                name: "ParcelImports");

            migrationBuilder.DropIndex(
                name: "IX_Parcels_ParcelImportId",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "ParcelImportId",
                table: "Parcels");
        }
    }
}
