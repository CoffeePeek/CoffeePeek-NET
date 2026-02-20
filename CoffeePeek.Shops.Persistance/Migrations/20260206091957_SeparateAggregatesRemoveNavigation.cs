using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeparateAggregatesRemoveNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "BrewMethods",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "BrewMethods");
        }
    }
}
