using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_bag_add_shippingPảtner_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bags_ShippingPartnerId",
                table: "Bags",
                column: "ShippingPartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_ShippingPartners_ShippingPartnerId",
                table: "Bags",
                column: "ShippingPartnerId",
                principalTable: "ShippingPartners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bags_ShippingPartners_ShippingPartnerId",
                table: "Bags");

            migrationBuilder.DropIndex(
                name: "IX_Bags_ShippingPartnerId",
                table: "Bags");
        }
    }
}
