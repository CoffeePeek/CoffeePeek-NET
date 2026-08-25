using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationShopCoffeeFocus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoffeeFocus",
                table: "ModerationShops",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_CoffeeFocus",
                table: "ModerationShops",
                column: "CoffeeFocus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ModerationShops_CoffeeFocus",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "CoffeeFocus",
                table: "ModerationShops");
        }
    }
}
