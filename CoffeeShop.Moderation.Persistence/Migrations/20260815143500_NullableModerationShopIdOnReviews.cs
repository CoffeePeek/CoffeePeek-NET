using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NullableModerationShopIdOnReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationReviews_ModerationShops_ModerationShopId",
                table: "ModerationReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
                table: "ShopPhotos");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModerationShopId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModerationShopId",
                table: "ModerationReviews",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationReviews_ModerationShops_ModerationShopId",
                table: "ModerationReviews",
                column: "ModerationShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
                table: "ShopPhotos",
                column: "ModerationShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationReviews_ModerationShops_ModerationShopId",
                table: "ModerationReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
                table: "ShopPhotos");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModerationShopId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModerationShopId",
                table: "ModerationReviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationReviews_ModerationShops_ModerationShopId",
                table: "ModerationReviews",
                column: "ModerationShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
                table: "ShopPhotos",
                column: "ModerationShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
