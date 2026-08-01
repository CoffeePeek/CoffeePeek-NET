using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddShopTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeShopTags",
                columns: table => new
                {
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeShopTags", x => new { x.ShopId, x.TagId });
                    table.ForeignKey(
                        name: "FK_CoffeeShopTags_ShopTags_TagId",
                        column: x => x.TagId,
                        principalTable: "ShopTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoffeeShopTags_Shops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeShopTags_TagId",
                table: "CoffeeShopTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopTags_Slug",
                table: "ShopTags",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopTags_SortOrder",
                table: "ShopTags",
                column: "SortOrder");

            var seedUtc = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                table: "ShopTags",
                columns: new[] { "Id", "Slug", "Name", "Description", "SortOrder", "IsActive", "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-4000-8000-000000000001"), "laptop_friendly", "Laptop Friendly", "Suitable for working with a laptop", 1, true, seedUtc, null },
                    { new Guid("a1000000-0000-4000-8000-000000000002"), "specialty", "Specialty", "Specialty coffee focus", 2, true, seedUtc, null },
                    { new Guid("a1000000-0000-4000-8000-000000000003"), "pet_friendly", "Pet Friendly", "Pets are welcome", 3, true, seedUtc, null },
                    { new Guid("a1000000-0000-4000-8000-000000000004"), "pour_over", "Pour Over", "Pour-over brewing available", 4, true, seedUtc, null },
                    { new Guid("a1000000-0000-4000-8000-000000000005"), "quiet_work", "Quiet Work", "Quiet atmosphere for focused work", 5, true, seedUtc, null },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoffeeShopTags");

            migrationBuilder.DropTable(
                name: "ShopTags");
        }
    }
}
