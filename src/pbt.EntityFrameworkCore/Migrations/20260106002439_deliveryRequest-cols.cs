using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class deliveryRequestcols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bags_DeliveryRequests_DeliveryRequestId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryRequests_CustomerAddresses_AddressId",
                table: "DeliveryRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryRequests_Customers_CustomerId",
                table: "DeliveryRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_DeliveryRequests_DeliveryRequestId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_DeliveryRequestId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryRequests_AddressId",
                table: "DeliveryRequests");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryRequests_CustomerId",
                table: "DeliveryRequests");

            migrationBuilder.DropIndex(
                name: "IX_Bags_DeliveryRequestId",
                table: "Bags");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DeliveryRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "DeliveryRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "DeliveryRequests");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "DeliveryRequests");

            migrationBuilder.CreateIndex(
                name: "IX_Packages_DeliveryRequestId",
                table: "Packages",
                column: "DeliveryRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_AddressId",
                table: "DeliveryRequests",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_CustomerId",
                table: "DeliveryRequests",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bags_DeliveryRequestId",
                table: "Bags",
                column: "DeliveryRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_DeliveryRequests_DeliveryRequestId",
                table: "Bags",
                column: "DeliveryRequestId",
                principalTable: "DeliveryRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryRequests_CustomerAddresses_AddressId",
                table: "DeliveryRequests",
                column: "AddressId",
                principalTable: "CustomerAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryRequests_Customers_CustomerId",
                table: "DeliveryRequests",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_DeliveryRequests_DeliveryRequestId",
                table: "Packages",
                column: "DeliveryRequestId",
                principalTable: "DeliveryRequests",
                principalColumn: "Id");
        }
    }
}
