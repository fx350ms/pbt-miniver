using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class order_add_waybillid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "WaybillId",
                table: "Orders",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Waybills_ParentId",
                table: "Waybills",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CNWarehouseId",
                table: "Orders",
                column: "CNWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_VNWarehouseId",
                table: "Orders",
                column: "VNWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_WaybillId",
                table: "Orders",
                column: "WaybillId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders",
                column: "WaybillId",
                principalTable: "Waybills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Waybills_Waybills_ParentId",
                table: "Waybills",
                column: "ParentId",
                principalTable: "Waybills",
                principalColumn: "Id");
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

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Waybills_Waybills_ParentId",
                table: "Waybills");

            migrationBuilder.DropIndex(
                name: "IX_Waybills_ParentId",
                table: "Waybills");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CNWarehouseId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_VNWarehouseId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_WaybillId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "WaybillId",
                table: "Orders");
        }
    }
}
