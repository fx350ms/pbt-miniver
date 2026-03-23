using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class delivery_quest_add_order : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequestsOrders_OrderId",
                table: "DeliveryRequestsOrders",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryRequestsOrders_Orders_OrderId",
                table: "DeliveryRequestsOrders",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryRequestsOrders_Orders_OrderId",
                table: "DeliveryRequestsOrders");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryRequestsOrders_OrderId",
                table: "DeliveryRequestsOrders");
        }
    }
}
