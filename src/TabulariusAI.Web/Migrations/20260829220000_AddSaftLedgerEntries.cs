using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Adds persistence for SAF-T (PT) accounting transactions and their debit and credit lines.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829220000_AddSaftLedgerEntries")]
public partial class AddSaftLedgerEntries : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SaftTransactions",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                SaftImportId = table.Column<int>(type: "int", nullable: false),
                JournalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                JournalDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Period = table.Column<int>(type: "int", nullable: false),
                TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                SourceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                DocArchivalNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                TransactionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                GlPostingDate = table.Column<DateOnly>(type: "date", nullable: false),
                CustomerId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                SupplierId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SaftTransactions", x => x.Id);
                table.ForeignKey(name: "FK_SaftTransactions_SaftImports_SaftImportId", column: x => x.SaftImportId, principalTable: "SaftImports", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "SaftTransactionLines",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                SaftTransactionId = table.Column<int>(type: "int", nullable: false),
                RecordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                AccountId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                SourceDocumentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                SystemEntryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Side = table.Column<string>(type: "nchar(1)", fixedLength: true, maxLength: 1, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SaftTransactionLines", x => x.Id);
                table.ForeignKey(name: "FK_SaftTransactionLines_SaftTransactions_SaftTransactionId", column: x => x.SaftTransactionId, principalTable: "SaftTransactions", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "IX_SaftTransactions_SaftImportId_JournalId", table: "SaftTransactions", columns: new[] { "SaftImportId", "JournalId" });
        migrationBuilder.CreateIndex(name: "IX_SaftTransactions_SaftImportId_TransactionDate", table: "SaftTransactions", columns: new[] { "SaftImportId", "TransactionDate" });
        migrationBuilder.CreateIndex(name: "IX_SaftTransactions_SaftImportId_TransactionId", table: "SaftTransactions", columns: new[] { "SaftImportId", "TransactionId" }, unique: true);
        migrationBuilder.CreateIndex(name: "IX_SaftTransactionLines_AccountId", table: "SaftTransactionLines", column: "AccountId");
        migrationBuilder.CreateIndex(name: "IX_SaftTransactionLines_SaftTransactionId_RecordId", table: "SaftTransactionLines", columns: new[] { "SaftTransactionId", "RecordId" }, unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SaftTransactionLines");
        migrationBuilder.DropTable(name: "SaftTransactions");
    }
}
