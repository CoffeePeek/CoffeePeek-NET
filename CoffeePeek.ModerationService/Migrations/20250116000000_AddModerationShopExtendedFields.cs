using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.ModerationService.Migrations
{
    /// <inheritdoc />
    public partial class AddModerationShopExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add new columns to ModerationShops
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ModerationShops",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceRange",
                table: "ModerationShops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "ModerationShops",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "ModerationShops",
                type: "uuid",
                nullable: true);

            // Update ShopContacts - remove ShopId, add Email and SiteLink
            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "ShopContacts");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ShopContacts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteLink",
                table: "ShopContacts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ShopContacts",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18);

            migrationBuilder.AlterColumn<string>(
                name: "InstagramLink",
                table: "ShopContacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            // Update ShopPhotos - add UpdatedAt, make Url required (non-nullable)
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ShopPhotos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ShopPhotos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70);

            // Create ModerationLocations table
            migrationBuilder.CreateTable(
                name: "ModerationLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationLocations_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ModerationShopEquipments table
            migrationBuilder.CreateTable(
                name: "ModerationShopEquipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShopEquipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShopEquipments_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ModerationCoffeeBeanShops table
            migrationBuilder.CreateTable(
                name: "ModerationCoffeeBeanShops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoffeeBeanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationCoffeeBeanShops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationCoffeeBeanShops_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ModerationRoasterShops table
            migrationBuilder.CreateTable(
                name: "ModerationRoasterShops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoasterId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationRoasterShops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationRoasterShops_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create ModerationShopBrewMethods table
            migrationBuilder.CreateTable(
                name: "ModerationShopBrewMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    BrewMethodId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShopBrewMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShopBrewMethods_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_CityId",
                table: "ModerationShops",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_LocationId",
                table: "ModerationShops",
                column: "LocationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLocations_ShopId",
                table: "ModerationLocations",
                column: "ShopId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopEquipments_ShopId",
                table: "ModerationShopEquipments",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopEquipments_EquipmentId",
                table: "ModerationShopEquipments",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCoffeeBeanShops_ShopId",
                table: "ModerationCoffeeBeanShops",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCoffeeBeanShops_CoffeeBeanId",
                table: "ModerationCoffeeBeanShops",
                column: "CoffeeBeanId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRoasterShops_ShopId",
                table: "ModerationRoasterShops",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRoasterShops_RoasterId",
                table: "ModerationRoasterShops",
                column: "RoasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopBrewMethods_ShopId",
                table: "ModerationShopBrewMethods",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopBrewMethods_BrewMethodId",
                table: "ModerationShopBrewMethods",
                column: "BrewMethodId");

            // Add foreign key for LocationId
            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShops_ModerationLocations_LocationId",
                table: "ModerationShops",
                column: "LocationId",
                principalTable: "ModerationLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove foreign key
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationShops_ModerationLocations_LocationId",
                table: "ModerationShops");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_ModerationShopBrewMethods_BrewMethodId",
                table: "ModerationShopBrewMethods");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShopBrewMethods_ShopId",
                table: "ModerationShopBrewMethods");

            migrationBuilder.DropIndex(
                name: "IX_ModerationRoasterShops_RoasterId",
                table: "ModerationRoasterShops");

            migrationBuilder.DropIndex(
                name: "IX_ModerationRoasterShops_ShopId",
                table: "ModerationRoasterShops");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCoffeeBeanShops_CoffeeBeanId",
                table: "ModerationCoffeeBeanShops");

            migrationBuilder.DropIndex(
                name: "IX_ModerationCoffeeBeanShops_ShopId",
                table: "ModerationCoffeeBeanShops");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShopEquipments_EquipmentId",
                table: "ModerationShopEquipments");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShopEquipments_ShopId",
                table: "ModerationShopEquipments");

            migrationBuilder.DropIndex(
                name: "IX_ModerationLocations_ShopId",
                table: "ModerationLocations");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShops_LocationId",
                table: "ModerationShops");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShops_CityId",
                table: "ModerationShops");

            // Drop tables
            migrationBuilder.DropTable(
                name: "ModerationShopBrewMethods");

            migrationBuilder.DropTable(
                name: "ModerationRoasterShops");

            migrationBuilder.DropTable(
                name: "ModerationCoffeeBeanShops");

            migrationBuilder.DropTable(
                name: "ModerationShopEquipments");

            migrationBuilder.DropTable(
                name: "ModerationLocations");

            // Revert ShopPhotos changes
            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ShopPhotos",
                type: "character varying(70)",
                maxLength: 70,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ShopPhotos");

            // Revert ShopContacts changes
            migrationBuilder.AlterColumn<string>(
                name: "InstagramLink",
                table: "ShopContacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ShopContacts",
                type: "character varying(18)",
                maxLength: 18,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "SiteLink",
                table: "ShopContacts");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ShopContacts");

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "ShopContacts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Revert ModerationShops changes
            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ModerationShops");
        }
    }
}





