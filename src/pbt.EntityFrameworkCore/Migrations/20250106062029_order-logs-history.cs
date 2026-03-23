using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class orderlogshistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderHístories",
                table: "OrderHístories");

            migrationBuilder.RenameTable(
                name: "OrderHístories",
                newName: "OrderHistories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderHistories",
                table: "OrderHistories",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_OrderHistories",
                table: "OrderHistories");

            migrationBuilder.RenameTable(
                name: "OrderHistories",
                newName: "OrderHístories");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrderHístories",
                table: "OrderHístories",
                column: "Id");
        }
    }
}
