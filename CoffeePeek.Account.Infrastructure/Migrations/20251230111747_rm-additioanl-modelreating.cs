using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class rmadditioanlmodelreating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserCredentials",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_Email",
                table: "UserCredentials",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_EmailConfirmationToken",
                table: "UserCredentials",
                column: "EmailConfirmationToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserCredentials_Email",
                table: "UserCredentials");

            migrationBuilder.DropIndex(
                name: "IX_UserCredentials_EmailConfirmationToken",
                table: "UserCredentials");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "UserCredentials",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Roles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);
        }
    }
}
