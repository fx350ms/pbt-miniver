using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_deliveryNote_add_shipping_partner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShippingPartnerId",
                table: "DeliveryNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingPartnerId",
                table: "DeliveryNotes");
        }
    }
}
