using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Persistance.Migrations
{
    /// <inheritdoc />
    public partial class AddImportedFromFileAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ImportedFromFileAt",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_ImportedFromFileAt",
                table: "Shops",
                column: "ImportedFromFileAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Shops_ImportedFromFileAt",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "ImportedFromFileAt",
                table: "Shops");
        }
    }
}
