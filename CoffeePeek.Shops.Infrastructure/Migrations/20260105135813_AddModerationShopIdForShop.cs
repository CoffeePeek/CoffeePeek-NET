using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.ShopsService.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationShopIdForShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ModerationId",
                table: "Shops",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModerationId",
                table: "Shops");
        }
    }
}
