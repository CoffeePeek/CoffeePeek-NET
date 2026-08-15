using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopImportCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopImportCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: false),
                    Phone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Website = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    Instagram = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OpeningHours = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Cuisine = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Brand = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OsmUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OsmAgeDays = table.Column<int>(type: "integer", nullable: true),
                    CheckDate = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Signals = table.Column<string>(type: "jsonb", nullable: false),
                    CollectorBucket = table.Column<int>(type: "integer", nullable: false),
                    QueueStatus = table.Column<int>(type: "integer", nullable: false),
                    CoffeeFocus = table.Column<int>(type: "integer", nullable: true),
                    TagSlugs = table.Column<string>(type: "jsonb", nullable: false),
                    GoogleBusinessStatus = table.Column<int>(type: "integer", nullable: true),
                    GoogleMapsUri = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    GoogleFetchedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResultingShopId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopImportCandidates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportCandidates_CoffeeFocus",
                table: "ShopImportCandidates",
                column: "CoffeeFocus");

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportCandidates_CollectorBucket",
                table: "ShopImportCandidates",
                column: "CollectorBucket");

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportCandidates_QueueStatus",
                table: "ShopImportCandidates",
                column: "QueueStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportCandidates_Source_ExternalId",
                table: "ShopImportCandidates",
                columns: new[] { "Source", "ExternalId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopImportCandidates");
        }
    }
}
