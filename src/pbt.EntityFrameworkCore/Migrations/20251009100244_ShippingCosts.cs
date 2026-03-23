using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class ShippingCosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingCostGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingPartnerId = table.Column<int>(type: "int", nullable: false),
                    IsActived = table.Column<bool>(type: "bit", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingCostGroups", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "ShippingCostTiers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductTypeId = table.Column<int>(type: "int", nullable: false),
                    FromValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ToValue = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCostId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingCostTiers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingCostTiers_ShippingCosts_ShippingCostId",
                        column: x => x.ShippingCostId,
                        principalTable: "ShippingCosts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingCostTiers_ShippingCostId",
                table: "ShippingCostTiers",
                column: "ShippingCostId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingCostGroups");

            migrationBuilder.DropTable(
                name: "ShippingCostTiers");

            migrationBuilder.DropTable(
                name: "ShippingCosts");
        }
    }
}
