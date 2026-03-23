using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class exportimportdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "ShippingPartners");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "ShippingPartners");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "ShippingPartners");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ShippingPartners");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ShippingPartners");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "ShippingPartners");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "ShippingPartners");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportDate",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportDate",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExportDate",
                table: "Bags",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImportDate",
                table: "Bags",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExportDate",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ImportDate",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ExportDate",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "ImportDate",
                table: "Bags");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "ShippingPartners",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "ShippingPartners",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "ShippingPartners",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ShippingPartners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ShippingPartners",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "ShippingPartners",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "ShippingPartners",
                type: "bigint",
                nullable: true);
        }
    }
}
