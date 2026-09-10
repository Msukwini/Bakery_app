using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakeryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeDeliveryAssignmentIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryEarnings_DeliveryAssignments_DeliveryAssignmentId",
                table: "DeliveryEarnings");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryAssignmentId",
                table: "DeliveryEarnings",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryEarnings_DeliveryAssignments_DeliveryAssignmentId",
                table: "DeliveryEarnings",
                column: "DeliveryAssignmentId",
                principalTable: "DeliveryAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryEarnings_DeliveryAssignments_DeliveryAssignmentId",
                table: "DeliveryEarnings");

            migrationBuilder.AlterColumn<Guid>(
                name: "DeliveryAssignmentId",
                table: "DeliveryEarnings",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryEarnings_DeliveryAssignments_DeliveryAssignmentId",
                table: "DeliveryEarnings",
                column: "DeliveryAssignmentId",
                principalTable: "DeliveryAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
