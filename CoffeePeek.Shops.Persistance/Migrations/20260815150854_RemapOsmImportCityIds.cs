using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class RemapOsmImportCityIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OSM import wrote CitiesConsts.MinskId, which is not the Cities row id.
            // Search/list filter by the real city; map does not — so imported shops vanished from the feed.
            migrationBuilder.Sql(
                """
                UPDATE "Shops" AS s
                SET "CityId" = c."Id"
                FROM "Cities" AS c
                WHERE lower(c."Name") = lower('Минск')
                  AND s."CityId" <> c."Id"
                  AND (
                        s."CityId" = 'd3fe962f-b1aa-42c3-b3b0-ee59322d0b6b'
                        OR NOT EXISTS (SELECT 1 FROM "Cities" AS x WHERE x."Id" = s."CityId")
                      );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
