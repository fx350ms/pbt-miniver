using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class bagexportvndate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Bags",
                type: "datetime2",
                nullable: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_Bags_DeliveryNoteId",
            //    table: "Bags",
            //    column: "DeliveryNoteId");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Bags_DeliveryNotes_DeliveryNoteId",
            //    table: "Bags",
            //    column: "DeliveryNoteId",
            //    principalTable: "DeliveryNotes",
            //    principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Bags_DeliveryNotes_DeliveryNoteId",
            //    table: "Bags");

            //migrationBuilder.DropIndex(
            //    name: "IX_Bags_DeliveryNoteId",
            //    table: "Bags");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Bags");
        }
    }
}
