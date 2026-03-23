using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_deliveryNote_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Size",
                table: "DeliveryNotes",
                newName: "width");

            migrationBuilder.AddColumn<int>(
                name: "DeliveryNoteId",
                table: "Packages",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Note",
                table: "DeliveryNotes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "DeliveryFee",
                table: "DeliveryNotes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "DeliveryNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "length",
                table: "DeliveryNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryNoteId",
                table: "Bags",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryNoteId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeliveryFee",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "height",
                table: "DeliveryNotes");

            migrationBuilder.DropColumn(
                name: "length",
                table: "DeliveryNotes");

            migrationBuilder.RenameColumn(
                name: "width",
                table: "DeliveryNotes",
                newName: "Size");

            migrationBuilder.AlterColumn<int>(
                name: "Note",
                table: "DeliveryNotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryNoteId",
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
