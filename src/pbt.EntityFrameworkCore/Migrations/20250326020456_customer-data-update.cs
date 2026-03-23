using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class customerdataupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddressReceipt",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "CustomerFakes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CustomerFakes");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_WarehouseId",
                table: "Packages",
                column: "WarehouseId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Packages_Warehouses_WarehouseId",
            //    table: "Packages",
            //    column: "WarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Warehouses_WarehouseId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_WarehouseId",
                table: "Packages");

            migrationBuilder.AddColumn<string>(
                name: "AddressReceipt",
                table: "CustomerFakes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "CustomerFakes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "CustomerFakes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "CustomerFakes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "CustomerFakes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "CustomerFakes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CustomerFakes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "CustomerFakes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerFakes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "CustomerFakes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "CustomerFakes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CustomerFakes",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
