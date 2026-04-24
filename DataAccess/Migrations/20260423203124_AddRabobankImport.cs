using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddRabobankImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transaction",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RabobankImport",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Volgnr = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IBAN = table.Column<string>(type: "varchar(34)", maxLength: 34, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Datum = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Bedrag = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    SaldoNaTrn = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TegenrekeningIBAN = table.Column<string>(type: "varchar(34)", maxLength: 34, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NaamTegenpartij = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Omschrijving = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ImportedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "enum('Pending','Processed','Skipped','Duplicate')", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    SuggestedBudgetLineId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RabobankImport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RabobankImport_BudgetLine_SuggestedBudgetLineId",
                        column: x => x.SuggestedBudgetLineId,
                        principalTable: "BudgetLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RabobankImport_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_rabobankimport_volgnr",
                table: "RabobankImport",
                column: "Volgnr",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RabobankImport_SuggestedBudgetLineId",
                table: "RabobankImport",
                column: "SuggestedBudgetLineId");

            migrationBuilder.CreateIndex(
                name: "IX_RabobankImport_TransactionId",
                table: "RabobankImport",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RabobankImport");

            migrationBuilder.UpdateData(
                table: "Transaction",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Transaction",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
