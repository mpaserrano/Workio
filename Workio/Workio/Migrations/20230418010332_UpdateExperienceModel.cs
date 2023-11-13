using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workio.Migrations
{
    public partial class UpdateExperienceModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExperienceModel_AspNetUsers_UserId",
                table: "ExperienceModel");

            migrationBuilder.DropColumn(
                name: "ExperienceUserId",
                table: "ExperienceModel");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ExperienceModel",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ExperienceModel_AspNetUsers_UserId",
                table: "ExperienceModel",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExperienceModel_AspNetUsers_UserId",
                table: "ExperienceModel");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ExperienceModel",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "ExperienceUserId",
                table: "ExperienceModel",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_ExperienceModel_AspNetUsers_UserId",
                table: "ExperienceModel",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
