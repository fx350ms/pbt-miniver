using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class removewaybill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Waybills_Orders_OrderId",
                table: "Waybills");

            migrationBuilder.DropForeignKey(
                name: "FK_Waybills_Waybills_ParentId",
                table: "Waybills");

            migrationBuilder.DropIndex(
                name: "IX_Waybills_OrderId",
                table: "Waybills");

            migrationBuilder.DropIndex(
                name: "IX_Waybills_ParentId",
                table: "Waybills");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Waybills");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "Waybills");

            migrationBuilder.RenameColumn(
                name: "WaybillId",
                table: "Orders",
                newName: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ParentId",
                table: "Orders",
                newName: "WaybillId");

            migrationBuilder.AddColumn<long>(
                name: "OrderId",
                table: "Waybills",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "Waybills",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Waybills_OrderId",
                table: "Waybills",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Waybills_ParentId",
                table: "Waybills",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders",
                column: "WaybillId",
                principalTable: "Waybills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Waybills_Orders_OrderId",
                table: "Waybills",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Waybills_Waybills_ParentId",
                table: "Waybills",
                column: "ParentId",
                principalTable: "Waybills",
                principalColumn: "Id");
        }
    }
}
