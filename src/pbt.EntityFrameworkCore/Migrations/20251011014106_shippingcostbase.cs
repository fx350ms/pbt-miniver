using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class shippingcostbase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingCostTiers_ShippingCosts_ShippingCostId",
                table: "ShippingCostTiers");

            migrationBuilder.DropTable(
                name: "ShippingCosts");

            migrationBuilder.RenameColumn(
                name: "ShippingCostId",
                table: "ShippingCostTiers",
                newName: "ShippingCostBaseId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingCostTiers_ShippingCostId",
                table: "ShippingCostTiers",
                newName: "IX_ShippingCostTiers_ShippingCostBaseId");

            migrationBuilder.CreateTable(
                name: "ShippingCostBases",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingCostGroupId = table.Column<long>(type: "bigint", nullable: false),
                    ShippingTypeId = table.Column<int>(type: "int", nullable: false),
                    WarehouseFromId = table.Column<int>(type: "int", nullable: false),
                    WarehouseToId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingCostBases", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingCostTiers_ShippingCostBases_ShippingCostBaseId",
                table: "ShippingCostTiers",
                column: "ShippingCostBaseId",
                principalTable: "ShippingCostBases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShippingCostTiers_ShippingCostBases_ShippingCostBaseId",
                table: "ShippingCostTiers");

            migrationBuilder.DropTable(
                name: "ShippingCostBases");

            migrationBuilder.RenameColumn(
                name: "ShippingCostBaseId",
                table: "ShippingCostTiers",
                newName: "ShippingCostId");

            migrationBuilder.RenameIndex(
                name: "IX_ShippingCostTiers_ShippingCostBaseId",
                table: "ShippingCostTiers",
                newName: "IX_ShippingCostTiers_ShippingCostId");

            migrationBuilder.CreateTable(
                name: "ShippingCosts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShippingCostGroupId = table.Column<long>(type: "bigint", nullable: false),
                    ShippingTypeId = table.Column<int>(type: "int", nullable: false),
                    WarehouseFromId = table.Column<int>(type: "int", nullable: false),
                    WarehouseToId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingCosts", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingCostTiers_ShippingCosts_ShippingCostId",
                table: "ShippingCostTiers",
                column: "ShippingCostId",
                principalTable: "ShippingCosts",
                principalColumn: "Id");
        }
    }
}
