using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class warehousetransferdata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BagIds",
                table: "WarehouseTransfers");

            migrationBuilder.RenameColumn(
                name: "PackageIds",
                table: "WarehouseTransfers",
                newName: "Data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "WarehouseTransfers",
                newName: "PackageIds");

            migrationBuilder.AddColumn<string>(
                name: "BagIds",
                table: "WarehouseTransfers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
