using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addModerationReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModerationReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Header = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    RatingCoffee = table.Column<int>(type: "integer", nullable: false),
                    RatingPlace = table.Column<int>(type: "integer", nullable: false),
                    RatingService = table.Column<int>(type: "integer", nullable: false),
                    RejectedReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ModeratedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ModeratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false),
                    IsSoftDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationReviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReviews_ModeratedBy",
                table: "ModerationReviews",
                column: "ModeratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReviews_ModerationStatus",
                table: "ModerationReviews",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReviews_ShopId",
                table: "ModerationReviews",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationReviews_UserId",
                table: "ModerationReviews",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationReviews");
        }
    }
}
