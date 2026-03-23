using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class changeaudittoe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "ShippingRateCustomers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "ShippingRateCustomers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "ShippingRateCustomers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ShippingRateCustomers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "ShippingRateCustomers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: true);
        }
    }
}
