using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Reviews",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ShopSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ShopSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ShopScheduleIntervals",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ShopScheduleIntervals",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Shops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ShopPhotos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ShopPhotos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ShopEquipments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ShopEquipments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ShopContacts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ShopContacts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ShopBrewMethods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ShopBrewMethods",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "RoasterShops",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "RoasterShops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Roasters",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Roasters",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "Reviews",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Reviews",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Locations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Locations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CoffeeBeanShops",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "CoffeeBeanShops",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "CoffeeBeans",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "CoffeeBeans",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "Cities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "Cities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "BrewMethods",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "BrewMethods",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ShopSchedules");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ShopSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ShopScheduleIntervals");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ShopScheduleIntervals");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ShopEquipments");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ShopEquipments");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ShopContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ShopContacts");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ShopBrewMethods");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ShopBrewMethods");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "RoasterShops");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "RoasterShops");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Roasters");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Roasters");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CoffeeBeanShops");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "CoffeeBeanShops");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "CoffeeBeans");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "CoffeeBeans");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "BrewMethods");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "BrewMethods");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Reviews",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Header",
                table: "Reviews",
                type: "character varying(70)",
                maxLength: 70,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
