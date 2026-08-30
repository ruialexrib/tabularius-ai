using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Adds source-traceable SAF-T (PT) customer and supplier master data.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829190000_AddSaftParties")]
public partial class AddSaftParties : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        CreatePartyTable(migrationBuilder, "SaftCustomers", "CustomerId", "IX_SaftCustomers_SaftImportId_CustomerId");
        CreatePartyTable(migrationBuilder, "SaftSuppliers", "SupplierId", "IX_SaftSuppliers_SaftImportId_SupplierId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "SaftCustomers");
        migrationBuilder.DropTable(name: "SaftSuppliers");
    }

    private static void CreatePartyTable(MigrationBuilder migrationBuilder, string tableName, string sourceIdColumn, string indexName)
    {
        migrationBuilder.CreateTable(name: tableName, columns: table => new
        {
            Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1").Annotation("Sqlite:Autoincrement", true),
            SaftImportId = table.Column<int>(nullable: false),
            SourceId = table.Column<string>(name: sourceIdColumn, maxLength: 100, nullable: false),
            AccountId = table.Column<string>(maxLength: 100, nullable: false),
            TaxId = table.Column<string>(maxLength: 30, nullable: false),
            CompanyName = table.Column<string>(maxLength: 300, nullable: false)
        }, constraints: table =>
        {
            table.PrimaryKey($"PK_{tableName}", x => x.Id);
            table.ForeignKey(name: $"FK_{tableName}_SaftImports_SaftImportId", column: x => x.SaftImportId, principalTable: "SaftImports", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        });
        migrationBuilder.CreateIndex(name: indexName, table: tableName, columns: new[] { "SaftImportId", sourceIdColumn }, unique: true);
    }
}
