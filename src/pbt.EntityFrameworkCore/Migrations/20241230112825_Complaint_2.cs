using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class Complaint_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bag_Customers_CustomerId",
                table: "Bag");

            migrationBuilder.DropForeignKey(
                name: "FK_Bag_Warehouses_WarehouseCreateId",
                table: "Bag");

            migrationBuilder.DropForeignKey(
                name: "FK_Bag_Warehouses_WarehouseDestinationId",
                table: "Bag");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Bag_BagId",
                table: "Packages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bag",
                table: "Bag");

            migrationBuilder.RenameTable(
                name: "Bag",
                newName: "Bags");

            migrationBuilder.RenameIndex(
                name: "IX_Bag_WarehouseDestinationId",
                table: "Bags",
                newName: "IX_Bags_WarehouseDestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Bag_WarehouseCreateId",
                table: "Bags",
                newName: "IX_Bags_WarehouseCreateId");

            migrationBuilder.RenameIndex(
                name: "IX_Bag_CustomerId",
                table: "Bags",
                newName: "IX_Bags_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bags",
                table: "Bags",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ComplaintImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintId = table.Column<long>(type: "bigint", nullable: false),
                    ImageData = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplaintImages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Complaints",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComplaintCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundRequest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Complaints", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Customers_CustomerId",
                table: "Bags",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Warehouses_WarehouseCreateId",
                table: "Bags",
                column: "WarehouseCreateId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_Warehouses_WarehouseDestinationId",
                table: "Bags",
                column: "WarehouseDestinationId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Bags_BagId",
                table: "Packages",
                column: "BagId",
                principalTable: "Bags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Customers_CustomerId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Warehouses_WarehouseCreateId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Bags_Warehouses_WarehouseDestinationId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_Bags_BagId",
                table: "Packages");

            migrationBuilder.DropTable(
                name: "ComplaintImages");

            migrationBuilder.DropTable(
                name: "Complaints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bags",
                table: "Bags");

            migrationBuilder.RenameTable(
                name: "Bags",
                newName: "Bag");

            migrationBuilder.RenameIndex(
                name: "IX_Bags_WarehouseDestinationId",
                table: "Bag",
                newName: "IX_Bag_WarehouseDestinationId");

            migrationBuilder.RenameIndex(
                name: "IX_Bags_WarehouseCreateId",
                table: "Bag",
                newName: "IX_Bag_WarehouseCreateId");

            migrationBuilder.RenameIndex(
                name: "IX_Bags_CustomerId",
                table: "Bag",
                newName: "IX_Bag_CustomerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bag",
                table: "Bag",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bag_Customers_CustomerId",
                table: "Bag",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bag_Warehouses_WarehouseCreateId",
                table: "Bag",
                column: "WarehouseCreateId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bag_Warehouses_WarehouseDestinationId",
                table: "Bag",
                column: "WarehouseDestinationId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_Bag_BagId",
                table: "Packages",
                column: "BagId",
                principalTable: "Bag",
                principalColumn: "Id");
        }
    }
}
