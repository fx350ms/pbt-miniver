using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class add_woodenPacking_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "WoodenPackingId",
                table: "Packages",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WoodenPackings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WoodenPackingCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeightTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VolumeTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WoodenPackings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WoodenPackings");

            migrationBuilder.DropColumn(
                name: "WoodenPackingId",
                table: "Packages");
        }
    }
}
