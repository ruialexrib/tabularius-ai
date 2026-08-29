using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>
/// Adds source-traceable SAF-T (PT) customer and supplier master data.
/// </summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829190000_AddSaftParties")]
public partial class AddSaftParties : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        CreatePartyTable(migrationBuilder, "SaftCustomers", "CustomerId", "IX_SaftCustomers_SaftImportId_CustomerId");
        CreatePartyTable(migrationBuilder, "SaftSuppliers", "SupplierId", "IX_SaftSuppliers_SaftImportId_SupplierId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SaftCustomers");
        migrationBuilder.DropTable(name: "SaftSuppliers");
    }

    /// <summary>Creates a SAF-T party master-data table with a unique source identifier per import.</summary>
    /// <param name="migrationBuilder">The active migration builder.</param>
    /// <param name="tableName">The database table name.</param>
    /// <param name="sourceIdColumn">The customer or supplier source identifier column.</param>
    /// <param name="indexName">The unique source index name.</param>
    private static void CreatePartyTable(MigrationBuilder migrationBuilder, string tableName, string sourceIdColumn, string indexName)
    {
        migrationBuilder.CreateTable(name: tableName, columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
            SaftImportId = table.Column<int>(type: "int", nullable: false),
            SourceId = table.Column<string>(name: sourceIdColumn, type: "nvarchar(100)", maxLength: 100, nullable: false),
            AccountId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            TaxId = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
            CompanyName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey($"PK_{tableName}", x => x.Id);
            table.ForeignKey(name: $"FK_{tableName}_SaftImports_SaftImportId", column: x => x.SaftImportId, principalTable: "SaftImports", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex(name: indexName, table: tableName, columns: new[] { "SaftImportId", sourceIdColumn }, unique: true);
    }
}
