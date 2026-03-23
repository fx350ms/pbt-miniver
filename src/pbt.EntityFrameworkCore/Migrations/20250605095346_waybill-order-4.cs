using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class waybillorder4 : Migration
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
                name: "IX_Orders_WaybillId",
                table: "Orders",
                column: "WaybillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
                table: "Orders",
                column: "WaybillId",
                principalTable: "Waybills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Waybills_WaybillId",
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
