using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_delivery_request_foregikey_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequestsOrders_DeliveryRequestId",
                table: "DeliveryRequestsOrders",
                column: "DeliveryRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryRequestsOrders_DeliveryRequests_DeliveryRequestId",
                table: "DeliveryRequestsOrders",
                column: "DeliveryRequestId",
                principalTable: "DeliveryRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryRequestsOrders_DeliveryRequests_DeliveryRequestId",
                table: "DeliveryRequestsOrders");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryRequestsOrders_DeliveryRequestId",
                table: "DeliveryRequestsOrders");
        }
    }
}
