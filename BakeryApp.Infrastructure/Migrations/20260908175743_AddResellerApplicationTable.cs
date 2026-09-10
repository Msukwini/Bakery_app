using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BakeryApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddResellerApplicationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ResellerApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: false),
                    LastName = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: false),
                    ResidenceName = table.Column<string>(type: "TEXT", nullable: true),
                    RoomNumber = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedResidencePopulation = table.Column<int>(type: "INTEGER", nullable: true),
                    PreferredSellingArea = table.Column<string>(type: "TEXT", nullable: true),
                    PreviousSalesExperience = table.Column<string>(type: "TEXT", nullable: true),
                    Availability = table.Column<string>(type: "TEXT", nullable: true),
                    ExpectedTimeAtResidence = table.Column<string>(type: "TEXT", nullable: true),
                    AdditionalInfo = table.Column<string>(type: "TEXT", nullable: true),
                    ResidenceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AdminNotes = table.Column<string>(type: "TEXT", nullable: true),
                    ReviewedByAdminId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ApprovedEmployeeId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResellerApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResellerApplications_EmployeeIds_ApprovedEmployeeId",
                        column: x => x.ApprovedEmployeeId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResellerApplications_EmployeeIds_ReviewedByAdminId",
                        column: x => x.ReviewedByAdminId,
                        principalTable: "EmployeeIds",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ResellerApplications_Residences_ResidenceId",
                        column: x => x.ResidenceId,
                        principalTable: "Residences",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ResellerApplications_ApprovedEmployeeId",
                table: "ResellerApplications",
                column: "ApprovedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResellerApplications_ResidenceId",
                table: "ResellerApplications",
                column: "ResidenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResellerApplications_ReviewedByAdminId",
                table: "ResellerApplications",
                column: "ReviewedByAdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ResellerApplications");
        }
    }
}
