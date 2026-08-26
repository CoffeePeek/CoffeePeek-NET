using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuDraftJson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Menu",
                table: "ShopImportCandidates",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Menu",
                table: "ModerationShops",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Menu",
                table: "ShopImportCandidates");

            migrationBuilder.DropColumn(
                name: "Menu",
                table: "ModerationShops");
        }
    }
}
