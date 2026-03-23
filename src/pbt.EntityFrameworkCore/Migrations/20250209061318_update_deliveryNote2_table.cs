using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_deliveryNote2_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExportTime",
                table: "DeliveryNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExportWarehouse",
                table: "DeliveryNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExporterId",
                table: "DeliveryNotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExportTime",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "ExportWarehouse",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "ExporterId",
                table: "DeliveryNotes");
        }
    }
}
