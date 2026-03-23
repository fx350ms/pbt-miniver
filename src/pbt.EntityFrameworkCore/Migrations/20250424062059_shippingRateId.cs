using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class shippingRateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.DropColumn(
                name: "ShipingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.AlterColumn<long>(
                name: "ShippingRateGroupId",
                table: "ShippingRates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRates",
                column: "ShippingRateGroupId",
                principalTable: "ShippingRateGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRates");

            migrationBuilder.AlterColumn<long>(
                name: "ShippingRateGroupId",
                table: "ShippingRates",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "ShipingRateGroupId",
                table: "ShippingRates",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_ShippingRateGroups_ShippingRateGroupId",
                table: "ShippingRates",
                column: "ShippingRateGroupId",
                principalTable: "ShippingRateGroups",
                principalColumn: "Id");
        }
    }
}
