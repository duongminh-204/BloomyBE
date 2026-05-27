using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BloomyBE.Migrations
{
    public partial class AddPortfolioPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "PortfolioItems",
                type: "decimal(18,2)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "PortfolioItems");
        }
    }
}
