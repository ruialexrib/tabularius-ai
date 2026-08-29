using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>
/// Adds a stable content identity used to prevent duplicate SAF-T (PT) imports.
/// </summary>
public partial class AddSaftContentHash : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "ContentHash", table: "SaftImports", type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: true);
        migrationBuilder.CreateIndex(name: "IX_SaftImports_ContentHash", table: "SaftImports", column: "ContentHash", unique: true, filter: "[ContentHash] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_SaftImports_ContentHash", table: "SaftImports");
        migrationBuilder.DropColumn(name: "ContentHash", table: "SaftImports");
    }
}
