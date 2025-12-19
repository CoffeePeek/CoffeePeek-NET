using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.ModerationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleExceptions");

            migrationBuilder.DropTable(
                name: "Schedules");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "ShopContacts");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ShopPhotos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(70)",
                oldMaxLength: 70);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ShopPhotos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "ModerationShops",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ModerationShops",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "ModerationShops",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriceRange",
                table: "ModerationShops",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateTable(
                name: "ModerationRoasterShops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoasterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ModerationShopBrewMethods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BrewMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ModerationShopSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    ModerationShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShopSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShopSchedules_ModerationShops_ModerationShopId",
                        column: x => x.ModerationShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModerationShopScheduleIntervals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShopScheduleIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShopScheduleIntervals_ModerationShopSchedules_Sch~",
                        column: x => x.ScheduleId,
                        principalTable: "ModerationShopSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_CityId",
                table: "ModerationShops",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCoffeeBeanShops_CoffeeBeanId",
                table: "ModerationCoffeeBeanShops",
                column: "CoffeeBeanId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationCoffeeBeanShops_ShopId",
                table: "ModerationCoffeeBeanShops",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationLocations_ShopId",
                table: "ModerationLocations",
                column: "ShopId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRoasterShops_RoasterId",
                table: "ModerationRoasterShops",
                column: "RoasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationRoasterShops_ShopId",
                table: "ModerationRoasterShops",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopBrewMethods_BrewMethodId",
                table: "ModerationShopBrewMethods",
                column: "BrewMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopBrewMethods_ShopId",
                table: "ModerationShopBrewMethods",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopEquipments_EquipmentId",
                table: "ModerationShopEquipments",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopEquipments_ShopId",
                table: "ModerationShopEquipments",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopScheduleIntervals_ScheduleId",
                table: "ModerationShopScheduleIntervals",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopSchedules_ModerationShopId",
                table: "ModerationShopSchedules",
                column: "ModerationShopId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModerationCoffeeBeanShops");

            migrationBuilder.DropTable(
                name: "ModerationLocations");

            migrationBuilder.DropTable(
                name: "ModerationRoasterShops");

            migrationBuilder.DropTable(
                name: "ModerationShopBrewMethods");

            migrationBuilder.DropTable(
                name: "ModerationShopEquipments");

            migrationBuilder.DropTable(
                name: "ModerationShopScheduleIntervals");

            migrationBuilder.DropTable(
                name: "ModerationShopSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ModerationShops_CityId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ShopPhotos");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "ShopContacts");

            migrationBuilder.DropColumn(
                name: "SiteLink",
                table: "ShopContacts");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ModerationShops");

            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "ModerationShops");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "ShopPhotos",
                type: "character varying(70)",
                maxLength: 70,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "ShopContacts",
                type: "character varying(18)",
                maxLength: 18,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(18)",
                oldMaxLength: 18,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InstagramLink",
                table: "ShopContacts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ShopId",
                table: "ShopContacts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ScheduleExceptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosingTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExceptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OpeningTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleExceptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleExceptions_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosingTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpeningTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedules_ModerationShops_ShopId",
                        column: x => x.ShopId,
                        principalTable: "ModerationShops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleExceptions_ShopId",
                table: "ScheduleExceptions",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_ShopId",
                table: "Schedules",
                column: "ShopId");
        }
    }
}
