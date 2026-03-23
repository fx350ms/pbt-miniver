using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class warehousetransferdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Waybills_WaybillId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_WaybillId",
                table: "Packages");

            migrationBuilder.AddColumn<int>(
                name: "LastBagId",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUnbagTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferWarehouseTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentWarehouseId",
                table: "Bags",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferWarehouseTime",
                table: "Bags",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastBagId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "LastUnbagTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "TransferWarehouseTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CurrentWarehouseId",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "TransferWarehouseTime",
                table: "Bags");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_WaybillId",
                table: "Packages",
                column: "WaybillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Waybills_WaybillId",
                table: "Packages",
                column: "WaybillId",
                principalTable: "Waybills",
                principalColumn: "Id");
        }
    }
}
