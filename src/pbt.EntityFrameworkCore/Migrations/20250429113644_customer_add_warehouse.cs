using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class customer_add_warehouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_WarehouseId",
                table: "Customers",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Warehouses_WarehouseId",
                table: "Customers",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Warehouses_WarehouseId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_WarehouseId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Customers");
        }
    }
}
