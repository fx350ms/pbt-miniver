using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class IsRepresentForDeliveryNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRepresentForDeliveryNote",
                table: "Packages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepresentForDeliveryNote",
                table: "Bags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_DeliveryNoteId",
                table: "Packages",
                column: "DeliveryNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_DeliveryNotes_DeliveryNoteId",
                table: "Packages",
                column: "DeliveryNoteId",
                principalTable: "DeliveryNotes",
                onDelete: ReferentialAction.SetNull,
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_DeliveryNotes_DeliveryNoteId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_DeliveryNoteId",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "IsRepresentForDeliveryNote",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "IsRepresentForDeliveryNote",
                table: "Bags");
        }
    }
}
