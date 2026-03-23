using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class bag_export_vn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AddColumn<DateTime>(
            //    name: "ExportDateVn",
            //    table: "Bags",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Packages_DeliveryNoteId",
            //    table: "Packages",
            //    column: "DeliveryNoteId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Packages_DeliveryNotes_DeliveryNoteId",
            //    table: "Packages",
            //    column: "DeliveryNoteId",
            //    principalTable: "DeliveryNotes",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Packages_DeliveryNotes_DeliveryNoteId",
            //    table: "Packages");

            //migrationBuilder.DropIndex(
            //    name: "IX_Packages_DeliveryNoteId",
            //    table: "Packages");

            //migrationBuilder.DropColumn(
            //    name: "ExportDateVn",
            //    table: "Bags");
        }
    }
}
