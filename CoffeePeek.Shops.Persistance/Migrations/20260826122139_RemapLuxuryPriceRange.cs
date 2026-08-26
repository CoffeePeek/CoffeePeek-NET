using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RemapLuxuryPriceRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Luxury (4) is no longer a price level; fold into Expensive (3).
            migrationBuilder.Sql(
                """
                UPDATE "Shops"
                SET "PriceRange" = 3
                WHERE "PriceRange" = 4;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cannot restore which rows were Luxury vs Expensive.
        }
    }
}
