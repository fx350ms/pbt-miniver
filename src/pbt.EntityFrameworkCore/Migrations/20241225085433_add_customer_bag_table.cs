using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class add_customer_bag_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "CustomerId",
                table: "Bags",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bags_CustomerId",
                table: "Bags",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Customers_CustomerId",
                table: "Bags",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Customers_CustomerId",
                table: "Bags");

            migrationBuilder.DropIndex(
                name: "IX_Bags_CustomerId",
                table: "Bags");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Bags",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
