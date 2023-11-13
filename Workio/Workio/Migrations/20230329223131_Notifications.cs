using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workio.Migrations
{
    public partial class Notifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Localizations",
                keyColumn: "LocalizationId",
                keyValue: new Guid("201ce5d2-cd64-42a1-9269-3d44864f89ed"));

            migrationBuilder.DeleteData(
                table: "Localizations",
                keyColumn: "LocalizationId",
                keyValue: new Guid("2be1a51a-73b2-44b3-897b-283f592fe842"));

            migrationBuilder.DeleteData(
                table: "ReportReason",
                keyColumn: "Id",
                keyValue: new Guid("0c5232ca-a49f-4634-b366-59c8431f322d"));

            migrationBuilder.DeleteData(
                table: "ReportReason",
                keyColumn: "Id",
                keyValue: new Guid("248beb7a-8068-4971-b70b-4236c4ada95f"));

            migrationBuilder.DeleteData(
                table: "ReportReason",
                keyColumn: "Id",
                keyValue: new Guid("2c322f1a-41a8-40b1-ab63-eb050665bbfc"));

            migrationBuilder.DeleteData(
                table: "ReportReason",
                keyColumn: "Id",
                keyValue: new Guid("52b3ea49-8620-4920-a84d-41a62dfc3f97"));

            migrationBuilder.DeleteData(
                table: "ReportReason",
                keyColumn: "Id",
                keyValue: new Guid("712c7c8f-443c-44c1-aded-fd0c38b235a5"));

            migrationBuilder.DeleteData(
                table: "ReportReason",
                keyColumn: "Id",
                keyValue: new Guid("d9869891-88e3-44a8-96a6-70b636152605"));

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.InsertData(
                table: "Localizations",
                columns: new[] { "LocalizationId", "IconName", "Language" },
                values: new object[,]
                {
                    { new Guid("201ce5d2-cd64-42a1-9269-3d44864f89ed"), "Portugal", "Português" },
                    { new Guid("2be1a51a-73b2-44b3-897b-283f592fe842"), "UK", "English" }
                });

            migrationBuilder.InsertData(
                table: "ReportReason",
                columns: new[] { "Id", "Reason", "ReasonType" },
                values: new object[,]
                {
                    { new Guid("0c5232ca-a49f-4634-b366-59c8431f322d"), "Event doesnt exist", 2 },
                    { new Guid("248beb7a-8068-4971-b70b-4236c4ada95f"), "Bad Name", 2 },
                    { new Guid("2c322f1a-41a8-40b1-ab63-eb050665bbfc"), "Bad Name", 0 },
                    { new Guid("52b3ea49-8620-4920-a84d-41a62dfc3f97"), "Team members", 1 },
                    { new Guid("712c7c8f-443c-44c1-aded-fd0c38b235a5"), "NSFW Profile Picture", 0 },
                    { new Guid("d9869891-88e3-44a8-96a6-70b636152605"), "Bad Name", 1 }
                });
        }
    }
}
