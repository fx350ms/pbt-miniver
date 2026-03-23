using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class addviplevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Customers",
                newName: "VipLevel");

            migrationBuilder.AddColumn<int>(
                name: "AgentLevel",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentLevel",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "VipLevel",
                table: "Customers",
                newName: "Level");
        }
    }
}
