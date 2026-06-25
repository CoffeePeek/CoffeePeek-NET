using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunitySocialFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityCityFollows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityCityFollows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityReactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReactionType = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityReactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommunityUserFollows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowingUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityUserFollows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCityFollows_CityId",
                table: "CommunityCityFollows",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCityFollows_UserId",
                table: "CommunityCityFollows",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCityFollows_UserId_CityId",
                table: "CommunityCityFollows",
                columns: new[] { "UserId", "CityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityReactions_TargetType_TargetId",
                table: "CommunityReactions",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityReactions_UserId_TargetType_TargetId",
                table: "CommunityReactions",
                columns: new[] { "UserId", "TargetType", "TargetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityUserFollows_FollowerId",
                table: "CommunityUserFollows",
                column: "FollowerId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityUserFollows_FollowerId_FollowingUserId",
                table: "CommunityUserFollows",
                columns: new[] { "FollowerId", "FollowingUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityUserFollows_FollowingUserId",
                table: "CommunityUserFollows",
                column: "FollowingUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityCityFollows");

            migrationBuilder.DropTable(
                name: "CommunityReactions");

            migrationBuilder.DropTable(
                name: "CommunityUserFollows");
        }
    }
}
