using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class Bagtotalpackagefee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DomesticShippingFee",
                table: "Bags",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFee",
                table: "Bags",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShockproofFee",
                table: "Bags",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WoodenPackagingFee",
                table: "Bags",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DomesticShippingFee",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "InsuranceFee",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "ShockproofFee",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "WoodenPackagingFee",
                table: "Bags");
        }
    }
}
