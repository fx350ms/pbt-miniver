using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class order_remove_waybillid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_CNWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_VNWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WaybillId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WaybillId",
                table: "Orders");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Warehouses_CNWarehouseId",
                table: "Orders",
                column: "CNWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Warehouses_VNWarehouseId",
                table: "Orders",
                column: "VNWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_CNWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_VNWarehouseId",
                table: "Orders");

            migrationBuilder.AddColumn<long>(
                name: "WaybillId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WaybillId",
                table: "Orders",
                column: "WaybillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Warehouses_CNWarehouseId",
                table: "Orders",
                column: "CNWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Warehouses_VNWarehouseId",
                table: "Orders",
                column: "VNWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders",
                column: "WaybillId",
                principalTable: "Waybills",
                principalColumn: "Id");
        }
    }
}
