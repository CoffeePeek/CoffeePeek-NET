using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdReviewNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Shops_ShopId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "ShopId",
                table: "Reviews",
                newName: "CoffeeShopId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_ShopId",
                table: "Reviews",
                newName: "IX_Reviews_CoffeeShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Shops_CoffeeShopId",
                table: "Reviews",
                column: "CoffeeShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Shops_CoffeeShopId",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "CoffeeShopId",
                table: "Reviews",
                newName: "ShopId");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CoffeeShopId",
                table: "Reviews",
                newName: "IX_Reviews_ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Shops_ShopId",
                table: "Reviews",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
