using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.ShopsService.Migrations
{
    /// <inheritdoc />
    public partial class UpdPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shops_Cities_CityId",
                table: "Shops");

            migrationBuilder.DropForeignKey(
                name: "FK_Shops_FavoriteShops_FavoriteShopId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_CityId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_FavoriteShopId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "FavoriteShopId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "ShopPhotos");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ShopPhotos",
                newName: "OwnerId");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "ShopPhotos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ShopPhotos",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "ShopPhotos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "StorageKey",
                table: "ShopPhotos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteShops_ShopId",
                table: "FavoriteShops",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteShops_Shops_ShopId",
                table: "FavoriteShops",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteShops_Shops_ShopId",
                table: "FavoriteShops");

            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropIndex(
                name: "IX_FavoriteShops_ShopId",
                table: "FavoriteShops");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "StorageKey",
                table: "ShopPhotos");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "ShopPhotos",
                newName: "UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "FavoriteShopId",
                table: "Shops",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "ShopPhotos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_CityId",
                table: "Shops",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Shops_FavoriteShopId",
                table: "Shops",
                column: "FavoriteShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_Cities_CityId",
                table: "Shops",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_FavoriteShops_FavoriteShopId",
                table: "Shops",
                column: "FavoriteShopId",
                principalTable: "FavoriteShops",
                principalColumn: "Id");
        }
    }
}
