using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

[DbContext(typeof(TabulariusDbContext))]
[Migration("20260831010000_AddAiHomeCacheMinutes")]
public partial class AddAiHomeCacheMinutes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name:"HomeCacheMinutes",table:"AiSettings",type:migrationBuilder.ActiveProvider.Contains("Sqlite")?"INTEGER":"int",nullable:false,defaultValue:60);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name:"HomeCacheMinutes",table:"AiSettings");
    }
}
