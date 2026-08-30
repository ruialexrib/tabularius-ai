using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TabulariusAI.Web.Migrations;

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
