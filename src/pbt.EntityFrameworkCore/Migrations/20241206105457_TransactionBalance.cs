using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class TransactionBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfterTransaction",
                table: "WalletTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfterTransaction",
                table: "DebtTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BalanceAfterTransaction",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfterTransaction",
                table: "DebtTransactions");
        }
    }
}
