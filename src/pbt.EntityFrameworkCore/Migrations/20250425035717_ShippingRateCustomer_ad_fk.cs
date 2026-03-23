using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class ShippingRateCustomer_ad_fk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShippingRateCustomers_CustomerId",
                table: "ShippingRateCustomers",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRateCustomers_Customers_CustomerId",
                table: "ShippingRateCustomers",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRateCustomers_Customers_CustomerId",
                table: "ShippingRateCustomers");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRateCustomers_CustomerId",
                table: "ShippingRateCustomers");
        }
    }
}
