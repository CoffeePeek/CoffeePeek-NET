using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAvgRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rating_AverageRating",
                table: "Reviews",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Rating_AverageRating",
                table: "CheckIns",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating_AverageRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Rating_AverageRating",
                table: "CheckIns");
        }
    }
}
