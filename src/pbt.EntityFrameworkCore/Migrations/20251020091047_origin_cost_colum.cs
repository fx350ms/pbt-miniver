using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class origin_cost_colum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "OriginShippingCost",
                table: "Packages",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitType",
                table: "Packages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOriginPackageShippingCost",
                table: "Bags",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalOriginShippingCost",
                table: "Bags",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginShippingCost",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "UnitType",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "TotalOriginPackageShippingCost",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "TotalOriginShippingCost",
                table: "Bags");
        }
    }
}
