using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChekIns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_Shops_CoffeeShopId",
                table: "ShopPhotos");

            migrationBuilder.DropTable(
                name: "UserVisits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "ReviewDate",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "RatingService",
                table: "Reviews",
                newName: "Rating_Service");

            migrationBuilder.RenameColumn(
                name: "RatingPlace",
                table: "Reviews",
                newName: "Rating_Place");

            migrationBuilder.RenameColumn(
                name: "RatingCoffee",
                table: "Reviews",
                newName: "Rating_Coffee");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoffeeShopId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CheckInId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating_Coffee",
                table: "CheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rating_Place",
                table: "CheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rating_Service",
                table: "CheckIns",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VisitedAt",
                table: "CheckIns",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ShopPhotos_CheckInId",
                table: "ShopPhotos",
                column: "CheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPhotos_ReviewId",
                table: "ShopPhotos",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_CheckIns_CheckInId",
                table: "ShopPhotos",
                column: "CheckInId",
                principalTable: "CheckIns",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_Reviews_ReviewId",
                table: "ShopPhotos",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_Shops_CoffeeShopId",
                table: "ShopPhotos",
                column: "CoffeeShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_CheckIns_CheckInId",
                table: "ShopPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_Reviews_ReviewId",
                table: "ShopPhotos");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_Shops_CoffeeShopId",
                table: "ShopPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ShopPhotos_CheckInId",
                table: "ShopPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ShopPhotos_ReviewId",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "CheckInId",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "ReviewId",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "Rating_Coffee",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "Rating_Place",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "Rating_Service",
                table: "CheckIns");

            migrationBuilder.DropColumn(
                name: "VisitedAt",
                table: "CheckIns");

            migrationBuilder.RenameColumn(
                name: "Rating_Service",
                table: "Reviews",
                newName: "RatingService");

            migrationBuilder.RenameColumn(
                name: "Rating_Place",
                table: "Reviews",
                newName: "RatingPlace");

            migrationBuilder.RenameColumn(
                name: "Rating_Coffee",
                table: "Reviews",
                newName: "RatingCoffee");

            migrationBuilder.AlterColumn<Guid>(
                name: "CoffeeShopId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ShopPhotos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ShopPhotos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewDate",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "UserVisits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FirstVisitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HasReview = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastVisitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserVisits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserVisits_UserId",
                table: "UserVisits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserVisits_UserId_LastVisitedAt",
                table: "UserVisits",
                columns: new[] { "UserId", "LastVisitedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserVisits_UserId_ShopId",
                table: "UserVisits",
                columns: new[] { "UserId", "ShopId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserVisits_UserId_VisitCount",
                table: "UserVisits",
                columns: new[] { "UserId", "VisitCount" });

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_Shops_CoffeeShopId",
                table: "ShopPhotos",
                column: "CoffeeShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
