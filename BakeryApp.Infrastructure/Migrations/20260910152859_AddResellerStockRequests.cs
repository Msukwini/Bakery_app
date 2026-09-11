using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakeryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResellerStockRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResellerStockRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResellerEmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResellerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    AllocatedQuantity = table.Column<int>(type: "INTEGER", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedByAdminId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AllocatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AdminNotes = table.Column<string>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    ResellerNotes = table.Column<string>(type: "TEXT", nullable: true),
                    InventoryLedgerEntryId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResellerStockRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResellerStockRequests_EmployeeIds_ResellerId",
                        column: x => x.ResellerId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResellerStockRequests_EmployeeIds_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResellerStockRequests_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResellerStockRequests_ProductVariantId",
                table: "ResellerStockRequests",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ResellerStockRequests_ResellerId",
                table: "ResellerStockRequests",
                column: "ResellerId");

            migrationBuilder.CreateIndex(
                name: "IX_ResellerStockRequests_ReviewedByAdminId",
                table: "ResellerStockRequests",
                column: "ReviewedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResellerStockRequests");
        }
    }
}
