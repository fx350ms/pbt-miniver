using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class add_package_bagid_table_package : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BagId",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_BagId",
                table: "Packages",
                column: "BagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Bags_BagId",
                table: "Packages",
                column: "BagId",
                principalTable: "Bags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Bags_BagId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_BagId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "BagId",
                table: "Packages");
        }
    }
}
