using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class schedulechange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationShopScheduleIntervals_ModerationShopSchedules_Sch~",
                table: "ModerationShopScheduleIntervals");

            migrationBuilder.RenameColumn(
                name: "ScheduleId",
                table: "ModerationShopScheduleIntervals",
                newName: "ModerationShopScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ModerationShopScheduleIntervals_ScheduleId",
                table: "ModerationShopScheduleIntervals",
                newName: "IX_ModerationShopScheduleIntervals_ModerationShopScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShopScheduleIntervals_ModerationShopSchedules_Mod~",
                table: "ModerationShopScheduleIntervals",
                column: "ModerationShopScheduleId",
                principalTable: "ModerationShopSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationShopScheduleIntervals_ModerationShopSchedules_Mod~",
                table: "ModerationShopScheduleIntervals");

            migrationBuilder.RenameColumn(
                name: "ModerationShopScheduleId",
                table: "ModerationShopScheduleIntervals",
                newName: "ScheduleId");

            migrationBuilder.RenameIndex(
                name: "IX_ModerationShopScheduleIntervals_ModerationShopScheduleId",
                table: "ModerationShopScheduleIntervals",
                newName: "IX_ModerationShopScheduleIntervals_ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShopScheduleIntervals_ModerationShopSchedules_Sch~",
                table: "ModerationShopScheduleIntervals",
                column: "ScheduleId",
                principalTable: "ModerationShopSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
