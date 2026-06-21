using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class EnsureShopOwnerUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Previous migration updated the model snapshot but did not add the column on existing databases.
            migrationBuilder.Sql("""
                ALTER TABLE "Shops" ADD COLUMN IF NOT EXISTS "OwnerUserId" uuid;
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_Shops_OwnerUserId" ON "Shops" ("OwnerUserId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "IX_Shops_OwnerUserId";
                """);

            migrationBuilder.Sql("""
                ALTER TABLE "Shops" DROP COLUMN IF EXISTS "OwnerUserId";
                """);
        }
    }
}
