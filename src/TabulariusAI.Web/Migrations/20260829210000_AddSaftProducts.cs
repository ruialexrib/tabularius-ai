using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Adds source-traceable SAF-T (PT) products and services to the local database.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260829210000_AddSaftProducts")]
public partial class AddSaftProducts : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(name: "SaftProducts", columns: table => new { Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"), SaftImportId = table.Column<int>(type: "int", nullable: false), ProductType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false), ProductCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false), ProductGroup = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true), ProductDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false), ProductNumberCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false) }, constraints: table => { table.PrimaryKey("PK_SaftProducts", x => x.Id); table.ForeignKey(name: "FK_SaftProducts_SaftImports_SaftImportId", column: x => x.SaftImportId, principalTable: "SaftImports", principalColumn: "Id", onDelete: ReferentialAction.Cascade); });
        migrationBuilder.CreateIndex(name: "IX_SaftProducts_SaftImportId_ProductCode", table: "SaftProducts", columns: new[] { "SaftImportId", "ProductCode" }, unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "SaftProducts");
}
