using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TabulariusAI.Web.Data;

#nullable disable

namespace TabulariusAI.Web.Migrations;

/// <summary>Adds persistent cookie-consent acceptance to application users.</summary>
[DbContext(typeof(TabulariusDbContext))]
[Migration("20260830235900_AddCookieConsent")]
public partial class AddCookieConsent : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(name: "CookieConsentAcceptedAt", table: "AspNetUsers", nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "CookieConsentAcceptedAt", table: "AspNetUsers");
    }
}
