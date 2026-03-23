using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class complaintassignToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AssignToUserId",
                table: "Complaints",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "AssignToUserName",
                table: "Complaints",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignToUserId",
                table: "Complaints");

            migrationBuilder.DropColumn(
                name: "AssignToUserName",
                table: "Complaints");
        }
    }
}
