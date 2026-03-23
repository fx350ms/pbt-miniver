using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class deliverynotevolumebalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "DeliveryNotes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceBefore",
                table: "DeliveryNotes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFee",
                table: "DeliveryNotes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVolume",
                table: "DeliveryNotes",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "BalanceBefore",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "TotalFee",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "TotalVolume",
                table: "DeliveryNotes");
        }
    }
}
