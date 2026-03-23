using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_package_add_waybill_id_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Orders_OrderId",
                table: "Packages");

            migrationBuilder.AlterColumn<long>(
                name: "OrderId",
                table: "Packages",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "WaybillId",
                table: "Packages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_WaybillId",
                table: "Packages",
                column: "WaybillId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Orders_OrderId",
                table: "Packages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Waybills_WaybillId",
                table: "Packages",
                column: "WaybillId",
                principalTable: "Waybills",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Orders_OrderId",
                table: "Packages");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Waybills_WaybillId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_WaybillId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "WaybillId",
                table: "Packages");

            migrationBuilder.AlterColumn<long>(
                name: "OrderId",
                table: "Packages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Orders_OrderId",
                table: "Packages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
