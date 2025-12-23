using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationShops_ShopContacts_ShopContactId",
                table: "ModerationShops");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ShopId",
                table: "ShopPhotos");

            migrationBuilder.DropTable(
                name: "ShopContacts");

            migrationBuilder.DropIndex(
                name: "IX_ShopPhotos_ShopId",
                table: "ShopPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShops_ShopContactId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "ShopContactId",
                table: "ModerationShops");

            migrationBuilder.RenameColumn(
                name: "ShopId",
                table: "ModerationShops",
                newName: "ModerationShopContactId");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "ModerationShops",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "RejectedReason",
                table: "ModerationShops",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModerationShopContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModerationShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    InstagramLink = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SiteLink = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ModerationShopId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShopContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShopContacts_ModerationShops_ModerationShopId1",
                        column: x => x.ModerationShopId1,
                        principalTable: "ModerationShops",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPhotos_ModerationShopId",
                table: "ShopPhotos",
                column: "ModerationShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_ModerationShopContactId",
                table: "ModerationShops",
                column: "ModerationShopContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopContacts_ModerationShopId1",
                table: "ModerationShopContacts",
                column: "ModerationShopId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShops_ModerationShopContacts_ModerationShopContac~",
                table: "ModerationShops",
                column: "ModerationShopContactId",
                principalTable: "ModerationShopContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
                table: "ShopPhotos",
                column: "ModerationShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationShops_ModerationShopContacts_ModerationShopContac~",
                table: "ModerationShops");

            migrationBuilder.DropForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
                table: "ShopPhotos");

            migrationBuilder.DropTable(
                name: "ModerationShopContacts");

            migrationBuilder.DropIndex(
                name: "IX_ShopPhotos_ModerationShopId",
                table: "ShopPhotos");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShops_ModerationShopContactId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "RejectedReason",
                table: "ModerationShops");

            migrationBuilder.RenameColumn(
                name: "ModerationShopContactId",
                table: "ModerationShops",
                newName: "ShopId");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "ShopPhotos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "ModerationShops",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "ModerationShops",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopContactId",
                table: "ModerationShops",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ShopContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InstagramLink = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    SiteLink = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopContacts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPhotos_ShopId",
                table: "ShopPhotos",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_ShopContactId",
                table: "ModerationShops",
                column: "ShopContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShops_ShopContacts_ShopContactId",
                table: "ModerationShops",
                column: "ShopContactId",
                principalTable: "ShopContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ShopPhotos_ModerationShops_ShopId",
                table: "ShopPhotos",
                column: "ShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
