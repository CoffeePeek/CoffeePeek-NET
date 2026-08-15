using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddCoffeeFocusAndFormatTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoffeeFocus",
                table: "Shops",
                type: "integer",
                nullable: true);

            var seedUtc = new DateTime(2026, 8, 15, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                table: "ShopTags",
                columns: new[] { "Id", "Slug", "Name", "Description", "SortOrder", "IsActive", "CreatedAtUtc", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("a1000000-0000-4000-8000-000000000006"), "to_go", "To Go", "Takeaway-first / window", 6, true, seedUtc, null },
                    { new Guid("a1000000-0000-4000-8000-000000000007"), "roastery", "Roastery", "They roast on site or as a brand", 7, true, seedUtc, null },
                    { new Guid("a1000000-0000-4000-8000-000000000008"), "bakery", "Bakery", "Bakery plus coffee", 8, true, seedUtc, null },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "ShopTags", keyColumn: "Id", keyValue: new Guid("a1000000-0000-4000-8000-000000000006"));
            migrationBuilder.DeleteData(table: "ShopTags", keyColumn: "Id", keyValue: new Guid("a1000000-0000-4000-8000-000000000007"));
            migrationBuilder.DeleteData(table: "ShopTags", keyColumn: "Id", keyValue: new Guid("a1000000-0000-4000-8000-000000000008"));

            migrationBuilder.DropColumn(
                name: "CoffeeFocus",
                table: "Shops");
        }
    }
}
