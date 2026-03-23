using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class productgrouptype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingRateTierCustomers",
                table: "ShippingRateTierCustomers");

            migrationBuilder.RenameTable(
                name: "ShippingRateTierCustomers",
                newName: "ProductGroupTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductGroupTypes",
                table: "ProductGroupTypes",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductGroupTypes",
                table: "ProductGroupTypes");

            migrationBuilder.RenameTable(
                name: "ProductGroupTypes",
                newName: "ShippingRateTierCustomers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingRateTierCustomers",
                table: "ShippingRateTierCustomers",
                column: "Id");
        }
    }
}
