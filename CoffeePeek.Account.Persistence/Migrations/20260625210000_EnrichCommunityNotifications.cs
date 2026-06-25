using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Account.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnrichCommunityNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                table: "CommunityNotifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DedupKey",
                table: "CommunityNotifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReactionType",
                table: "CommunityNotifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommunityNotifications_DedupKey",
                table: "CommunityNotifications",
                column: "DedupKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CommunityNotifications_DedupKey",
                table: "CommunityNotifications");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "CommunityNotifications");

            migrationBuilder.DropColumn(
                name: "DedupKey",
                table: "CommunityNotifications");

            migrationBuilder.DropColumn(
                name: "ReactionType",
                table: "CommunityNotifications");
        }
    }
}
