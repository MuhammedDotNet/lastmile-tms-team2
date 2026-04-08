using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LastMile.TMS.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBinLocationDeliveryZoneAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryZoneId",
                table: "BinLocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BinLocations_DeliveryZoneId",
                table: "BinLocations",
                column: "DeliveryZoneId",
                unique: true,
                filter: "\"DeliveryZoneId\" IS NOT NULL AND \"IsActive\" = TRUE");

            migrationBuilder.AddForeignKey(
                name: "FK_BinLocations_Zones_DeliveryZoneId",
                table: "BinLocations",
                column: "DeliveryZoneId",
                principalTable: "Zones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BinLocations_Zones_DeliveryZoneId",
                table: "BinLocations");

            migrationBuilder.DropIndex(
                name: "IX_BinLocations_DeliveryZoneId",
                table: "BinLocations");

            migrationBuilder.DropColumn(
                name: "DeliveryZoneId",
                table: "BinLocations");
        }
    }
}
