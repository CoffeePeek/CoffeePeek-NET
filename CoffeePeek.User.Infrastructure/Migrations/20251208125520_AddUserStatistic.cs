using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.UserService.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatistic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserStatisticId",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "UserStatistics",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReviewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AddedShopsCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatistics", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserStatistics_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserStatistics");

            migrationBuilder.DropColumn(
                name: "UserStatisticId",
                table: "Users");
        }
    }
}
