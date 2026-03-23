using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class addordercolumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DomesticShipping",
                table: "Orders",
                newName: "DomesticShippingFeeRMB");

            migrationBuilder.AlterColumn<decimal>(
                name: "WoodenPackagingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "CostFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DomesticShippingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceValue",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDomesticShipping",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInsured",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWoodenCrate",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginShippingCost",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceCN",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShockproofFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Volume",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightCover",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightCoverFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeightPackage",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WoodenPackingId",
                table: "Orders",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DomesticShippingFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InsuranceFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InsuranceValue",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsDomesticShipping",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsInsured",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsWoodenCrate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OriginShippingCost",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "PriceCN",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShockproofFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WeightCover",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WeightCoverFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WeightPackage",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WoodenPackingId",
                table: "Orders");

            migrationBuilder.RenameColumn(
                name: "DomesticShippingFeeRMB",
                table: "Orders",
                newName: "DomesticShipping");

            migrationBuilder.AlterColumn<decimal>(
                name: "WoodenPackagingFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);
        }
    }
}
