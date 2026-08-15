using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeeShop.Moderation.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportCandidateRejectReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RejectReason",
                table: "ShopImportCandidates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopImportCandidates_RejectReason",
                table: "ShopImportCandidates",
                column: "RejectReason");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShopImportCandidates_RejectReason",
                table: "ShopImportCandidates");

            migrationBuilder.DropColumn(
                name: "RejectReason",
                table: "ShopImportCandidates");
        }
    }
}
