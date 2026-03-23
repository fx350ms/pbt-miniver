using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class add_warehouse_fk_bag_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "WarehouseDestinationId",
                table: "Bags",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseCreateId",
                table: "Bags",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Bags_WarehouseCreateId",
                table: "Bags",
                column: "WarehouseCreateId");

            migrationBuilder.CreateIndex(
                name: "IX_Bags_WarehouseDestinationId",
                table: "Bags",
                column: "WarehouseDestinationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Warehouses_WarehouseCreateId",
                table: "Bags",
                column: "WarehouseCreateId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Warehouses_WarehouseDestinationId",
                table: "Bags",
                column: "WarehouseDestinationId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Warehouses_WarehouseCreateId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Warehouses_WarehouseDestinationId",
                table: "Bags");

            migrationBuilder.DropIndex(
                name: "IX_Bags_WarehouseCreateId",
                table: "Bags");

            migrationBuilder.DropIndex(
                name: "IX_Bags_WarehouseDestinationId",
                table: "Bags");

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseDestinationId",
                table: "Bags",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseCreateId",
                table: "Bags",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
