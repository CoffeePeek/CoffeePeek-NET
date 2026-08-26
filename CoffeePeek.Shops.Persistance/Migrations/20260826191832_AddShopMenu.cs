using System;
using CoffeePeek.Shops.Persistance.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddShopMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoffeeDrinkDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NameRu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Aliases = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeDrinkDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopMenus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CoffeeShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ParseStatus = table.Column<int>(type: "integer", nullable: false),
                    ParseError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SuggestedPriceRange = table.Column<int>(type: "integer", nullable: true),
                    UnmatchedJson = table.Column<string>(type: "text", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopMenus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopMenus_Shops_CoffeeShopId",
                        column: x => x.CoffeeShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopMenuItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopMenuId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Availability = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    VolumeMl = table.Column<int>(type: "integer", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    CustomName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopMenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopMenuItems_CoffeeDrinkDefinitions_DrinkDefinitionId",
                        column: x => x.DrinkDefinitionId,
                        principalTable: "CoffeeDrinkDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopMenuItems_ShopMenus_ShopMenuId",
                        column: x => x.ShopMenuId,
                        principalTable: "ShopMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopMenuPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopMenuId = table.Column<Guid>(type: "uuid", nullable: false),
                    MediaPhotoId = table.Column<Guid>(type: "uuid", nullable: true),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopMenuPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopMenuPhotos_ShopMenus_ShopMenuId",
                        column: x => x.ShopMenuId,
                        principalTable: "ShopMenus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeDrinkDefinitions_Slug",
                table: "CoffeeDrinkDefinitions",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeDrinkDefinitions_SortOrder",
                table: "CoffeeDrinkDefinitions",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ShopMenuItems_DrinkDefinitionId",
                table: "ShopMenuItems",
                column: "DrinkDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopMenuItems_ShopMenuId_DrinkDefinitionId",
                table: "ShopMenuItems",
                columns: new[] { "ShopMenuId", "DrinkDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopMenuPhotos_ShopMenuId",
                table: "ShopMenuPhotos",
                column: "ShopMenuId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopMenus_CoffeeShopId",
                table: "ShopMenus",
                column: "CoffeeShopId",
                unique: true);

            var seedUtc = new DateTime(2026, 8, 26, 0, 0, 0, DateTimeKind.Utc);
            foreach (var row in CoffeeDrinkSeed.Rows)
            {
                migrationBuilder.InsertData(
                    table: "CoffeeDrinkDefinitions",
                    columns: new[]
                    {
                        "Id", "Slug", "NameRu", "NameEn", "Category", "Kind", "Aliases",
                        "SortOrder", "IsActive", "CreatedAtUtc", "UpdatedAtUtc"
                    },
                    values: new object[]
                    {
                        row.Id, row.Slug, row.NameRu, row.NameEn, row.Category, 1, row.Aliases,
                        row.Sort, true, seedUtc, null
                    });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopMenuItems");

            migrationBuilder.DropTable(
                name: "ShopMenuPhotos");

            migrationBuilder.DropTable(
                name: "CoffeeDrinkDefinitions");

            migrationBuilder.DropTable(
                name: "ShopMenus");
        }
    }
}
