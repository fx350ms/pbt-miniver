using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class TransactionFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FileId",
                table: "CustomerTransactions");

            migrationBuilder.AddColumn<string>(
                name: "Files",
                table: "Transactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FileContent",
                table: "FileUploads",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Files",
                table: "CustomerTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Files",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "FileContent",
                table: "FileUploads");

            migrationBuilder.DropColumn(
                name: "Files",
                table: "CustomerTransactions");

            migrationBuilder.AddColumn<long>(
                name: "FileId",
                table: "Transactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileId",
                table: "CustomerTransactions",
                type: "bigint",
                nullable: true);
        }
    }
}
