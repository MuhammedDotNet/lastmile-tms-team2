using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInboundReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboundManifests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TruckIdentifier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DepotId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundManifests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundManifests_Depots_DepotId",
                        column: x => x.DepotId,
                        principalTable: "Depots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InboundManifestLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundManifestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundManifestLines_InboundManifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "InboundManifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InboundManifestLines_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InboundReceivingSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ConfirmedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundReceivingSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundReceivingSessions_InboundManifests_ManifestId",
                        column: x => x.ManifestId,
                        principalTable: "InboundManifests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InboundReceivingExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uuid", nullable: true),
                    ManifestLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    TrackingNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ExceptionType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundReceivingExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundReceivingExceptions_InboundManifestLines_ManifestLin~",
                        column: x => x.ManifestLineId,
                        principalTable: "InboundManifestLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InboundReceivingExceptions_InboundReceivingSessions_Session~",
                        column: x => x.SessionId,
                        principalTable: "InboundReceivingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InboundReceivingExceptions_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InboundReceivingScans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParcelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ManifestLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    Barcode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MatchType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboundReceivingScans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InboundReceivingScans_InboundManifestLines_ManifestLineId",
                        column: x => x.ManifestLineId,
                        principalTable: "InboundManifestLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InboundReceivingScans_InboundReceivingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "InboundReceivingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InboundReceivingScans_Parcels_ParcelId",
                        column: x => x.ParcelId,
                        principalTable: "Parcels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboundManifestLines_ManifestId_ParcelId",
                table: "InboundManifestLines",
                columns: new[] { "ManifestId", "ParcelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboundManifestLines_ParcelId",
                table: "InboundManifestLines",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundManifests_DepotId_Status",
                table: "InboundManifests",
                columns: new[] { "DepotId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_InboundManifests_ManifestNumber",
                table: "InboundManifests",
                column: "ManifestNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingExceptions_ManifestLineId",
                table: "InboundReceivingExceptions",
                column: "ManifestLineId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingExceptions_ParcelId",
                table: "InboundReceivingExceptions",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingExceptions_SessionId_ExceptionType_Barcode",
                table: "InboundReceivingExceptions",
                columns: new[] { "SessionId", "ExceptionType", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingScans_ManifestLineId",
                table: "InboundReceivingScans",
                column: "ManifestLineId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingScans_ParcelId",
                table: "InboundReceivingScans",
                column: "ParcelId");

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingScans_SessionId_Barcode",
                table: "InboundReceivingScans",
                columns: new[] { "SessionId", "Barcode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InboundReceivingSessions_ManifestId_Status",
                table: "InboundReceivingSessions",
                columns: new[] { "ManifestId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboundReceivingExceptions");

            migrationBuilder.DropTable(
                name: "InboundReceivingScans");

            migrationBuilder.DropTable(
                name: "InboundManifestLines");

            migrationBuilder.DropTable(
                name: "InboundReceivingSessions");

            migrationBuilder.DropTable(
                name: "InboundManifests");
        }
    }
}
