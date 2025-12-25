using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Auth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updrefreshtokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_UserCredentials_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LoginProvider",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RefreshTokens",
                newName: "UserCredentialId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserCredentialId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_UserCredentials_UserCredentialId",
                table: "RefreshTokens",
                column: "UserCredentialId",
                principalTable: "UserCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_UserCredentials_UserCredentialId",
                table: "RefreshTokens");

            migrationBuilder.RenameColumn(
                name: "UserCredentialId",
                table: "RefreshTokens",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserCredentialId",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.AddColumn<string>(
                name: "LoginProvider",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "RefreshTokens",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_UserCredentials_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "UserCredentials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
