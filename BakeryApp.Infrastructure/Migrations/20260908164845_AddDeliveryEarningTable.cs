using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakeryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryEarningTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryEarnings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeliveryEmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeliveryAssignmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AmountEarned = table.Column<decimal>(type: "TEXT", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsSettled = table.Column<bool>(type: "INTEGER", nullable: false),
                    EarningDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryEarnings_DeliveryAssignments_DeliveryAssignmentId",
                        column: x => x.DeliveryAssignmentId,
                        principalTable: "DeliveryAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryEarnings_EmployeeIds_DeliveryEmployeeId",
                        column: x => x.DeliveryEmployeeId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryEarnings_DeliveryAssignmentId",
                table: "DeliveryEarnings",
                column: "DeliveryAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryEarnings_DeliveryEmployeeId",
                table: "DeliveryEarnings",
                column: "DeliveryEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryEarnings");
        }
    }
}
