using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdEquipments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoffeeShopEquipments");

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Equipments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Equipments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CoffeeShopId",
                table: "Equipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "Equipments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimary",
                table: "Equipments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "Equipments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "EquipmentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipmentCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CategoryId",
                table: "Equipments",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CoffeeShopId",
                table: "Equipments",
                column: "CoffeeShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_EquipmentCategories_CategoryId",
                table: "Equipments",
                column: "CategoryId",
                principalTable: "EquipmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Shops_CoffeeShopId",
                table: "Equipments",
                column: "CoffeeShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_EquipmentCategories_CategoryId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Shops_CoffeeShopId",
                table: "Equipments");

            migrationBuilder.DropTable(
                name: "EquipmentCategories");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_CategoryId",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_CoffeeShopId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "CoffeeShopId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "IsPrimary",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "Equipments");

            migrationBuilder.CreateTable(
                name: "CoffeeShopEquipments",
                columns: table => new
                {
                    CoffeeShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeShopEquipments", x => new { x.CoffeeShopId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_CoffeeShopEquipments_Equipments_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoffeeShopEquipments_Shops_CoffeeShopId",
                        column: x => x.CoffeeShopId,
                        principalTable: "Shops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeShopEquipments_EquipmentId",
                table: "CoffeeShopEquipments",
                column: "EquipmentId");
        }
    }
}
