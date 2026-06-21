using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnsureModerationAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Previous migration updated the model snapshot but did not create the table on existing databases.
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "ModerationAuditLogs" (
                    "Id" uuid NOT NULL,
                    "EntityType" integer NOT NULL,
                    "EntityId" uuid NOT NULL,
                    "EntityName" character varying(255) NOT NULL,
                    "Action" integer NOT NULL,
                    "ModeratorUserId" uuid NOT NULL,
                    "Comment" character varying(1000),
                    "CreatedAtUtc" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_ModerationAuditLogs" PRIMARY KEY ("Id")
                );
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ModerationAuditLogs_CreatedAtUtc"
                    ON "ModerationAuditLogs" ("CreatedAtUtc");
                """);

            migrationBuilder.Sql("""
                CREATE INDEX IF NOT EXISTS "IX_ModerationAuditLogs_EntityType_EntityId"
                    ON "ModerationAuditLogs" ("EntityType", "EntityId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP TABLE IF EXISTS "ModerationAuditLogs";
                """);
        }
    }
}
