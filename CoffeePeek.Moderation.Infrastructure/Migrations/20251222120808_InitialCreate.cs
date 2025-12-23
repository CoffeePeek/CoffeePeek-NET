using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Processed = table.Column<bool>(type: "boolean", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopContacts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    InstagramLink = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SiteLink = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModerationShops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NotValidatedAddress = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShopContactId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: true),
                    PriceRange = table.Column<int>(type: "integer", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsAddressValidated = table.Column<bool>(type: "boolean", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShops_ShopContacts_ShopContactId",
                        column: x => x.ShopContactId,
                        principalTable: "ShopContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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
                name: "ShopPhotos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    StorageKey = table.Column<string>(type: "text", nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModerationShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopPhotos_ModerationShops_ShopId",
                        column: x => x.ShopId,
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
                name: "IX_ModerationShops_CityId",
                table: "ModerationShops",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_ModerationStatus",
                table: "ModerationShops",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_ShopContactId",
                table: "ModerationShops",
                column: "ShopContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_UserId",
                table: "ModerationShops",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopScheduleIntervals_ScheduleId",
                table: "ModerationShopScheduleIntervals",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopSchedules_ModerationShopId",
                table: "ModerationShopSchedules",
                column: "ModerationShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPhotos_ShopId",
                table: "ShopPhotos",
                column: "ShopId");
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
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "ShopPhotos");

            migrationBuilder.DropTable(
                name: "ModerationShopSchedules");

            migrationBuilder.DropTable(
                name: "ModerationShops");

            migrationBuilder.DropTable(
                name: "ShopContacts");
        }
    }
}
