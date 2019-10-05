using Microsoft.EntityFrameworkCore.Migrations;

namespace Tailwind.Trader.Auction.Api.Migrations
{
    public partial class AddImageType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageType",
                table: "ProductImagePaths",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "ProductImagePaths");
        }
    }
}
