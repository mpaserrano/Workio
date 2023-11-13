using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workio.Migrations
{
    public partial class ReportReasonsLocalization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportReasonLocalizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocalizationCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportReasonLocalizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportReasonLocalizations_ReportReason_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ReportReason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportReasonLocalizations_ReportId",
                table: "ReportReasonLocalizations",
                column: "ReportId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportReasonLocalizations");
        }
    }
}
