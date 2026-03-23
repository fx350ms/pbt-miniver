using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class userparent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ParentId",
                table: "AbpUsers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Packages_DeliveryRequestId",
                table: "Packages",
                column: "DeliveryRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Bags_DeliveryRequestId",
                table: "Bags",
                column: "DeliveryRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bags_DeliveryRequests_DeliveryRequestId",
                table: "Bags",
                column: "DeliveryRequestId",
                principalTable: "DeliveryRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Packages_DeliveryRequests_DeliveryRequestId",
                table: "Packages",
                column: "DeliveryRequestId",
                principalTable: "DeliveryRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bags_DeliveryRequests_DeliveryRequestId",
                table: "Bags");

            migrationBuilder.DropForeignKey(
                name: "FK_Packages_DeliveryRequests_DeliveryRequestId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Packages_DeliveryRequestId",
                table: "Packages");

            migrationBuilder.DropIndex(
                name: "IX_Bags_DeliveryRequestId",
                table: "Bags");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "AbpUsers");
        }
    }
}
