using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.JobVacancies.Migrations
{
    /// <inheritdoc />
    public partial class updatevacnciesuniq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobVacancies_ExternalId",
                table: "JobVacancies");

            migrationBuilder.CreateIndex(
                name: "IX_JobVacancies_ExternalId",
                table: "JobVacancies",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobVacancies_ExternalId",
                table: "JobVacancies");

            migrationBuilder.CreateIndex(
                name: "IX_JobVacancies_ExternalId",
                table: "JobVacancies",
                column: "ExternalId");
        }
    }
}
