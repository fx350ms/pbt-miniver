using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class updatecustomeraddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "CustomerAddresses");

            migrationBuilder.DropColumn(
                name: "WardId",
                table: "CustomerAddresses");

            migrationBuilder.RenameColumn(
                name: "IsReceiptAddress",
                table: "CustomerAddresses",
                newName: "IsDefaultForAllCustomer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsDefaultForAllCustomer",
                table: "CustomerAddresses",
                newName: "IsReceiptAddress");

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "CustomerAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "CustomerAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardId",
                table: "CustomerAddresses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
