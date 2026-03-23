using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class orderwarehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Orders_Warehouses_CNWarehouseId",
            //    table: "Orders");

            //migrationBuilder.DropForeignKey(
            //    name: "FK_Orders_Warehouses_VNWarehouseId",
            //    table: "Orders");

            //migrationBuilder.DropIndex(
            //    name: "IX_Orders_CNWarehouseId",
            //    table: "Orders");

            //migrationBuilder.DropIndex(
            //    name: "IX_Orders_VNWarehouseId",
            //    table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.CreateIndex(
            //    name: "IX_Orders_CNWarehouseId",
            //    table: "Orders",
            //    column: "CNWarehouseId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_Orders_VNWarehouseId",
            //    table: "Orders",
            //    column: "VNWarehouseId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Orders_Warehouses_CNWarehouseId",
            //    table: "Orders",
            //    column: "CNWarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Orders_Warehouses_VNWarehouseId",
            //    table: "Orders",
            //    column: "VNWarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Restrict);
        }
    }
}
