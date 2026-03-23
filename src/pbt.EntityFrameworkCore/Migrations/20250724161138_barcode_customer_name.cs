using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class barcode_customer_name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "width",
                table: "DeliveryNotes",
                newName: "Width");

            migrationBuilder.RenameColumn(
                name: "length",
                table: "DeliveryNotes",
                newName: "Length");

            migrationBuilder.RenameColumn(
                name: "height",
                table: "DeliveryNotes",
                newName: "Height");

            migrationBuilder.AlterColumn<long>(
                name: "ExporterId",
                table: "DeliveryNotes",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "BarCodes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "BarCodes");

            migrationBuilder.RenameColumn(
                name: "Width",
                table: "DeliveryNotes",
                newName: "width");

            migrationBuilder.RenameColumn(
                name: "Length",
                table: "DeliveryNotes",
                newName: "length");

            migrationBuilder.RenameColumn(
                name: "Height",
                table: "DeliveryNotes",
                newName: "height");

            migrationBuilder.AlterColumn<int>(
                name: "ExporterId",
                table: "DeliveryNotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
