using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addConfirm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailConfirmationToken",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailConfirmationToken",
                table: "Users");
        }
    }
}
