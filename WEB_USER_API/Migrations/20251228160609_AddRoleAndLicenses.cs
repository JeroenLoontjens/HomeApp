using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WEB_USER_API.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndLicenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            // 1) voeg Role kolom toe
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "TEXT",
                nullable: true);

            // 2) maak Licenses tabel aan
            migrationBuilder.CreateTable(
                name: "Licenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Licenses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Licenses_UserId",
                table: "Licenses",
                column: "UserId");

            // 3) backfill: geef bestaande users een standaard role voordat salary verwijderd wordt
            migrationBuilder.Sql("UPDATE Users SET Role = 'User' WHERE Role IS NULL");

            // 4) verwijder de oude Salary kolom
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Licenses");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Users",
                newName: "Salary");
        }
    }
}
