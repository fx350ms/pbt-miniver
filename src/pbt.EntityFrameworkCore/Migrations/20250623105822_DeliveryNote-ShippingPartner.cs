using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryNoteShippingPartner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ShippingPartnerId",
                table: "DeliveryNotes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryNotes_ShippingPartnerId",
                table: "DeliveryNotes",
                column: "ShippingPartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryNotes_ShippingPartners_ShippingPartnerId",
                table: "DeliveryNotes",
                column: "ShippingPartnerId",
                principalTable: "ShippingPartners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryNotes_ShippingPartners_ShippingPartnerId",
                table: "DeliveryNotes");

            migrationBuilder.DropIndex(
                name: "IX_DeliveryNotes_ShippingPartnerId",
                table: "DeliveryNotes");

            migrationBuilder.AlterColumn<int>(
                name: "ShippingPartnerId",
                table: "DeliveryNotes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
