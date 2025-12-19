using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoffeePeek.JobVacancies.Migrations
{
    /// <inheritdoc />
    public partial class updatenamecity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CityId",
                table: "JobVacancies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CityMapId",
                table: "JobVacancies",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CityName",
                table: "Cities",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_JobVacancies_CityMapId",
                table: "JobVacancies",
                column: "CityMapId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobVacancies_Cities_CityMapId",
                table: "JobVacancies",
                column: "CityMapId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobVacancies_Cities_CityMapId",
                table: "JobVacancies");

            migrationBuilder.DropIndex(
                name: "IX_JobVacancies_CityMapId",
                table: "JobVacancies");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "JobVacancies");

            migrationBuilder.DropColumn(
                name: "CityMapId",
                table: "JobVacancies");

            migrationBuilder.DropColumn(
                name: "CityName",
                table: "Cities");
        }
    }
}
