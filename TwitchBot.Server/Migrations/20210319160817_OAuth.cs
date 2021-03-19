using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitchBot.Server.Migrations
{
    public partial class OAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "Subscriptions",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "CallbackId",
                table: "Subscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Subscriptions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccessToken = table.Column<string>(type: "TEXT", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCredentials", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientCredentials");

            migrationBuilder.DropColumn(
                name: "CallbackId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Subscriptions",
                newName: "SubscriptionId");
        }
    }
}
