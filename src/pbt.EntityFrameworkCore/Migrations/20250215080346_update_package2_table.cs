using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_package2_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Packages_ShippingPartnerId",
                table: "Packages",
                column: "ShippingPartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_ShippingPartners_ShippingPartnerId",
                table: "Packages",
                column: "ShippingPartnerId",
                principalTable: "ShippingPartners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Packages_ShippingPartners_ShippingPartnerId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_ShippingPartnerId",
                table: "Packages");
        }
    }
}
