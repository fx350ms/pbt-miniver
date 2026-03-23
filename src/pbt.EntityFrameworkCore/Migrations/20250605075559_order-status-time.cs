using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pbt.Migrations
{
    /// <inheritdoc />
    public partial class orderstatustime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_CNWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_VNWarehouseId",
                table: "Orders");

            migrationBuilder.AddColumn<DateTime>(
                name: "ClearanceTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CustomerNotClaimingTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryInProgressTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryRequestTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InWarehouseTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LostTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnedTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReturningTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShippingTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaitingForClearanceTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaitingForDeliveryTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaitingForReturnTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WaitingForShippingTime",
                table: "Packages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComplaintTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InTransitTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InTransitToChinaWarehouseTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InTransitToVietnamWarehouseTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OrderCompletedTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RefundTime",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Orders_Warehouses_CNWarehouseId",
            //    table: "Orders",
            //    column: "CNWarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Orders_Warehouses_VNWarehouseId",
            //    table: "Orders",
            //    column: "VNWarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_CNWarehouseId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Warehouses_VNWarehouseId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ClearanceTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CompletedTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CustomerNotClaimingTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeliveryInProgressTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DeliveryRequestTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "InWarehouseTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "LostTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ReceivedTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ReturnedTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ReturningTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "ShippingTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "WaitingForClearanceTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "WaitingForDeliveryTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "WaitingForReturnTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "WaitingForShippingTime",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "CancelledTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ComplaintTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InTransitTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InTransitToChinaWarehouseTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "InTransitToVietnamWarehouseTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OrderCompletedTime",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RefundTime",
                table: "Orders");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Orders_Warehouses_CNWarehouseId",
            //    table: "Orders",
            //    column: "CNWarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id"  ,
            //      onDelete: ReferentialAction.Cascade);

            //migrationBuilder.AddForeignKey(
            //    name: "FK_Orders_Warehouses_VNWarehouseId",
            //    table: "Orders",
            //    column: "VNWarehouseId",
            //    principalTable: "Warehouses",
            //    principalColumn: "Id"  ,
            //      onDelete: ReferentialAction.Restrict);
        }
    }
}
