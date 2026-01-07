using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.Moderation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                });

            migrationBuilder.CreateTable(
                name: "ModerationLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsAddressValidated = table.Column<bool>(type: "boolean", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    ShopId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationLocations", x => x.Id);
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
                });

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
                });

            migrationBuilder.CreateTable(
                name: "ModerationShops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PriceRange = table.Column<int>(type: "integer", nullable: false),
                    ModerationStatus = table.Column<int>(type: "integer", nullable: false),
                    RejectedReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ModerationShopContactId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShops_ModerationShopContacts_ModerationShopContac~",
                        column: x => x.ModerationShopContactId,
                        principalTable: "ModerationShopContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    ModerationShopId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false)
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
                    UploadedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopPhotos_ModerationShops_ModerationShopId",
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
                    ModerationShopScheduleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModerationShopScheduleIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModerationShopScheduleIntervals_ModerationShopSchedules_Mod~",
                        column: x => x.ModerationShopScheduleId,
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
                name: "IX_ModerationShopContacts_ModerationShopId1",
                table: "ModerationShopContacts",
                column: "ModerationShopId1");

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
                name: "IX_ModerationShops_ModerationShopContactId",
                table: "ModerationShops",
                column: "ModerationShopContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_ModerationStatus",
                table: "ModerationShops",
                column: "ModerationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShops_UserId",
                table: "ModerationShops",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopScheduleIntervals_ModerationShopScheduleId",
                table: "ModerationShopScheduleIntervals",
                column: "ModerationShopScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModerationShopSchedules_ModerationShopId",
                table: "ModerationShopSchedules",
                column: "ModerationShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPhotos_ModerationShopId",
                table: "ShopPhotos",
                column: "ModerationShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationCoffeeBeanShops_ModerationShops_ShopId",
                table: "ModerationCoffeeBeanShops",
                column: "ShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationLocations_ModerationShops_ShopId",
                table: "ModerationLocations",
                column: "ShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationRoasterShops_ModerationShops_ShopId",
                table: "ModerationRoasterShops",
                column: "ShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShopBrewMethods_ModerationShops_ShopId",
                table: "ModerationShopBrewMethods",
                column: "ShopId",
                principalTable: "ModerationShops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ModerationShopContacts_ModerationShops_ModerationShopId1",
                table: "ModerationShopContacts",
                column: "ModerationShopId1",
                principalTable: "ModerationShops",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ModerationShopContacts_ModerationShops_ModerationShopId1",
                table: "ModerationShopContacts");

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
                name: "ModerationShopContacts");
        }
    }
}
