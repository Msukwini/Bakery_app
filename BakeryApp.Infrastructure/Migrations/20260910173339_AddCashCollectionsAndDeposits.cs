using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakeryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashCollectionsAndDeposits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashCollections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeliveryEmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    CollectedAmount = table.Column<decimal>(type: "TEXT", nullable: false),
                    Variance = table.Column<decimal>(type: "TEXT", nullable: false),
                    VarianceReason = table.Column<int>(type: "INTEGER", nullable: false),
                    VarianceNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReconciledByAdminId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AdminNotes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashCollections_EmployeeIds_DeliveryEmployeeId",
                        column: x => x.DeliveryEmployeeId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashCollections_EmployeeIds_ReconciledByAdminId",
                        column: x => x.ReconciledByAdminId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", nullable: false),
                    DeliveryEmployeeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CashCollectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    DepositDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BankName = table.Column<string>(type: "TEXT", nullable: true),
                    BankReference = table.Column<string>(type: "TEXT", nullable: true),
                    ProofFilePath = table.Column<string>(type: "TEXT", nullable: true),
                    ProofMimeType = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ReviewedByAdminId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", nullable: true),
                    AdminNotes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deposits_CashCollections_CashCollectionId",
                        column: x => x.CashCollectionId,
                        principalTable: "CashCollections",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Deposits_EmployeeIds_DeliveryEmployeeId",
                        column: x => x.DeliveryEmployeeId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Deposits_EmployeeIds_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashCollections_DeliveryEmployeeId",
                table: "CashCollections",
                column: "DeliveryEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CashCollections_ReconciledByAdminId",
                table: "CashCollections",
                column: "ReconciledByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_CashCollectionId",
                table: "Deposits",
                column: "CashCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_DeliveryEmployeeId",
                table: "Deposits",
                column: "DeliveryEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_ReviewedByAdminId",
                table: "Deposits",
                column: "ReviewedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "CashCollections");
        }
    }
}
