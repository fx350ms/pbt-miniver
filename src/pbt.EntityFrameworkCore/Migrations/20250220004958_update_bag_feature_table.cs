using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class update_bag_feature_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Feature",
                table: "Bags");

            migrationBuilder.AddColumn<bool>(
                name: "IsFakeGoods",
                table: "Bags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOtherFeature",
                table: "Bags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSolution",
                table: "Bags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWoodSealing",
                table: "Bags",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFakeGoods",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "IsOtherFeature",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "IsSolution",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "IsWoodSealing",
                table: "Bags");

            migrationBuilder.AddColumn<int>(
                name: "Feature",
                table: "Bags",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
