using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportDuplicateSuggestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShopImportDuplicateSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeftCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    RightCandidateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    DistanceMeters = table.Column<double>(type: "double precision", nullable: false),
                    Reasons = table.Column<string>(type: "jsonb", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopImportDuplicateSuggestions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportDuplicateSuggestions_LeftCandidateId_RightCandida~",
                table: "ShopImportDuplicateSuggestions",
                columns: new[] { "LeftCandidateId", "RightCandidateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportDuplicateSuggestions_Score",
                table: "ShopImportDuplicateSuggestions",
                column: "Score");

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportDuplicateSuggestions_Status",
                table: "ShopImportDuplicateSuggestions",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopImportDuplicateSuggestions");
        }
    }
}
