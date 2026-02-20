using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Shops.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Shops_CoffeeShopId",
                table: "Equipments");

            migrationBuilder.DropIndex(
                name: "IX_Equipments_CoffeeShopId",
                table: "Equipments");

            migrationBuilder.DropColumn(
                name: "CoffeeShopId",
                table: "Equipments");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EquipmentCategories",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoffeeShopEquipments");

            migrationBuilder.AddColumn<Guid>(
                name: "CoffeeShopId",
                table: "Equipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "EquipmentCategories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Equipments_CoffeeShopId",
                table: "Equipments",
                column: "CoffeeShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Shops_CoffeeShopId",
                table: "Equipments",
                column: "CoffeeShopId",
                principalTable: "Shops",
                principalColumn: "Id");
        }
    }
}
