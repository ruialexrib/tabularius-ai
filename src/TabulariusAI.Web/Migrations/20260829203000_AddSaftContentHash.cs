using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Adds a stable content identity used to prevent duplicate SAF-T (PT) imports.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829203000_AddSaftContentHash")]
public partial class AddSaftContentHash : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "ContentHash", table: "SaftImports", maxLength: 64, nullable: true);
        migrationBuilder.CreateIndex(name: "IX_SaftImports_ContentHash", table: "SaftImports", column: "ContentHash", unique: true, filter: ActiveProvider.Contains("SqlServer") ? "[ContentHash] IS NOT NULL" : null);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_SaftImports_ContentHash", table: "SaftImports");
        migrationBuilder.DropColumn(name: "ContentHash", table: "SaftImports");
    }
}
