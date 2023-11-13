using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Workio.Migrations
{
    public partial class ChangeConnectionModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User_Connection_Id",
                table: "Connections");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Connections",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Connections",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "RequestedUserId",
                table: "Connections",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_RequestedUserId",
                table: "Connections",
                column: "RequestedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Connections_UserId",
                table: "Connections",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_AspNetUsers_RequestedUserId",
                table: "Connections",
                column: "RequestedUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Connections_AspNetUsers_UserId",
                table: "Connections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Connections_AspNetUsers_RequestedUserId",
                table: "Connections");

            migrationBuilder.DropForeignKey(
                name: "FK_Connections_AspNetUsers_UserId",
                table: "Connections");

            migrationBuilder.DropIndex(
                name: "IX_Connections_RequestedUserId",
                table: "Connections");

            migrationBuilder.DropIndex(
                name: "IX_Connections_UserId",
                table: "Connections");

            migrationBuilder.DropColumn(
                name: "RequestedUserId",
                table: "Connections");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Connections",
                newName: "id");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Connections",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<Guid>(
                name: "User_Connection_Id",
                table: "Connections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
