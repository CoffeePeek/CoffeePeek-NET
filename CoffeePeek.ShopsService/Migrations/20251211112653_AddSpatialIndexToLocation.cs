using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.ShopsService.Migrations
{
    /// <inheritdoc />
    public partial class AddSpatialIndexToLocation : Migration
    {
        /// <summary>
        /// Создаёт составной индекс IX_Locations_Latitude_Longitude на колонках Latitude и Longitude таблицы Locations.
        /// </summary>
        /// <param name="migrationBuilder">Объект для построения операций миграции.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Locations_Latitude_Longitude",
                table: "Locations",
                columns: new[] { "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_Latitude_Longitude",
                table: "Locations");
        }
    }
}