using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rating_AverageRating",
                table: "ModerationReviews",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating_AverageRating",
                table: "ModerationReviews");
        }
    }
}
