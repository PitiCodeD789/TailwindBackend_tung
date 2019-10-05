using Microsoft.EntityFrameworkCore.Migrations;

namespace Tailwind.Trader.Payment.Api.Migrations
{
    public partial class fixPayerId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "Payer_ID",
                table: "Payments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "Payer_ID",
                table: "Payments",
                column: "PayerId");
        }
    }
}
