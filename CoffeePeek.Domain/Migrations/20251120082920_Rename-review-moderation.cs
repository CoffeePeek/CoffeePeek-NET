using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Renamereviewmoderation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleExceptions_ReviewShops_ReviewShopId",
                table: "ScheduleExceptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ReviewShops_ReviewShopId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhoto_ReviewShops_ReviewShopId",
                table: "ShopPhoto");

            migrationBuilder.RenameColumn(
                name: "ReviewShopId",
                table: "ShopPhoto",
                newName: "ModerationShopId");

            migrationBuilder.RenameIndex(
                name: "IX_ShopPhoto_ReviewShopId",
                table: "ShopPhoto",
                newName: "IX_ShopPhoto_ModerationShopId");

            migrationBuilder.RenameColumn(
                name: "ReviewShopId",
                table: "Schedules",
                newName: "ModerationShopId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_ReviewShopId",
                table: "Schedules",
                newName: "IX_Schedules_ModerationShopId");

            migrationBuilder.RenameColumn(
                name: "ReviewShopId",
                table: "ScheduleExceptions",
                newName: "ModerationShopId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleExceptions_ReviewShopId",
                table: "ScheduleExceptions",
                newName: "IX_ScheduleExceptions_ModerationShopId");

            migrationBuilder.RenameColumn(
                name: "ReviewStatus",
                table: "ReviewShops",
                newName: "ModerationStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleExceptions_ReviewShops_ModerationShopId",
                table: "ScheduleExceptions",
                column: "ModerationShopId",
                principalTable: "ReviewShops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ReviewShops_ModerationShopId",
                table: "Schedules",
                column: "ModerationShopId",
                principalTable: "ReviewShops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhoto_ReviewShops_ModerationShopId",
                table: "ShopPhoto",
                column: "ModerationShopId",
                principalTable: "ReviewShops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScheduleExceptions_ReviewShops_ModerationShopId",
                table: "ScheduleExceptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_ReviewShops_ModerationShopId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhoto_ReviewShops_ModerationShopId",
                table: "ShopPhoto");

            migrationBuilder.RenameColumn(
                name: "ModerationShopId",
                table: "ShopPhoto",
                newName: "ReviewShopId");

            migrationBuilder.RenameIndex(
                name: "IX_ShopPhoto_ModerationShopId",
                table: "ShopPhoto",
                newName: "IX_ShopPhoto_ReviewShopId");

            migrationBuilder.RenameColumn(
                name: "ModerationShopId",
                table: "Schedules",
                newName: "ReviewShopId");

            migrationBuilder.RenameIndex(
                name: "IX_Schedules_ModerationShopId",
                table: "Schedules",
                newName: "IX_Schedules_ReviewShopId");

            migrationBuilder.RenameColumn(
                name: "ModerationShopId",
                table: "ScheduleExceptions",
                newName: "ReviewShopId");

            migrationBuilder.RenameIndex(
                name: "IX_ScheduleExceptions_ModerationShopId",
                table: "ScheduleExceptions",
                newName: "IX_ScheduleExceptions_ReviewShopId");

            migrationBuilder.RenameColumn(
                name: "ModerationStatus",
                table: "ReviewShops",
                newName: "ReviewStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduleExceptions_ReviewShops_ReviewShopId",
                table: "ScheduleExceptions",
                column: "ReviewShopId",
                principalTable: "ReviewShops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_ReviewShops_ReviewShopId",
                table: "Schedules",
                column: "ReviewShopId",
                principalTable: "ReviewShops",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhoto_ReviewShops_ReviewShopId",
                table: "ShopPhoto",
                column: "ReviewShopId",
                principalTable: "ReviewShops",
                principalColumn: "Id");
        }
    }
}
