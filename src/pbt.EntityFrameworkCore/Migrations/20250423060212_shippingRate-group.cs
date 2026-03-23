using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class shippingRategroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRates_ShippingRateId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ShippingRates");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "ShippingRates");

            migrationBuilder.RenameColumn(
                name: "IsDefault",
                table: "ShippingRates",
                newName: "UseCumulativeFormula");

            migrationBuilder.RenameColumn(
                name: "IsActived",
                table: "ShippingRates",
                newName: "ManualPricing");

            migrationBuilder.AddColumn<int>(
                name: "ProductTypeId",
                table: "ShippingRateTiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ShipingRateGroupId",
                table: "ShippingRates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ShippingRateGroupId",
                table: "ShippingRates",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "ShippingRateId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ShippingRateGroupId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ShippingRateGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActived = table.Column<bool>(type: "bit", nullable: false),
                    IsDefaultForCustomer = table.Column<bool>(type: "bit", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingRateGroups", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_ShippingRateGroupId",
                table: "ShippingRates",
                column: "ShippingRateGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRateCustomers_ShippingRateGroupId",
                table: "ShippingRateCustomers",
                column: "ShippingRateGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRateCustomers",
                column: "ShippingRateGroupId",
                principalTable: "ShippingRateGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRates_ShippingRateId",
                table: "ShippingRateCustomers",
                column: "ShippingRateId",
                principalTable: "ShippingRates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRates",
                column: "ShippingRateGroupId",
                principalTable: "ShippingRateGroups",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRates_ShippingRateId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.DropTable(
                name: "ShippingRateGroups");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRates_ShippingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRateCustomers_ShippingRateGroupId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "ProductTypeId",
                table: "ShippingRateTiers");

            migrationBuilder.DropColumn(
                name: "ShipingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.DropColumn(
                name: "ShippingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.DropColumn(
                name: "ShippingRateGroupId",
                table: "ShippingRateCustomers");

            migrationBuilder.RenameColumn(
                name: "UseCumulativeFormula",
                table: "ShippingRates",
                newName: "IsDefault");

            migrationBuilder.RenameColumn(
                name: "ManualPricing",
                table: "ShippingRates",
                newName: "IsActived");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ShippingRates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductTypeId",
                table: "ShippingRates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "ShippingRateId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRates_ShippingRateId",
                table: "ShippingRateCustomers",
                column: "ShippingRateId",
                principalTable: "ShippingRates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
