using Microsoft.EntityFrameworkCore.Migrations;

namespace Tailwind.Trader.Auction.Api.Migrations
{
    public partial class AddDbb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HighestBidder",
                table: "Products",
                newName: "HighestBidderId");

            migrationBuilder.AddColumn<string>(
                name: "HighestBidderName",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProductWeight",
                table: "Products",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HighestBidderName",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductWeight",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "HighestBidderId",
                table: "Products",
                newName: "HighestBidder");
        }
    }
}
