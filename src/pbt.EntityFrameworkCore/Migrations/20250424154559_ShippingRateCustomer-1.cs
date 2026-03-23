using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class ShippingRateCustomer1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRateCustomers_Customers_CustomerId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRates_ShippingRateId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRateCustomers_CustomerId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRateCustomers_ShippingRateId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropColumn(
                name: "ShippingRateId",
                table: "ShippingRateCustomers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ShippingRateId",
                table: "ShippingRateCustomers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRateCustomers_CustomerId",
                table: "ShippingRateCustomers",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRateCustomers_ShippingRateId",
                table: "ShippingRateCustomers",
                column: "ShippingRateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRateCustomers_Customers_CustomerId",
                table: "ShippingRateCustomers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRateCustomers_ShippingRates_ShippingRateId",
                table: "ShippingRateCustomers",
                column: "ShippingRateId",
                principalTable: "ShippingRates",
                principalColumn: "Id");
        }
    }
}
