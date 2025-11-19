using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CoffeePeek.Domain.Migrations
{
    /// <inheritdoc />
    public partial class ReviewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewRatingCategories");

            migrationBuilder.DropTable(
                name: "RatingCategories");

            migrationBuilder.AddColumn<decimal>(
                name: "RatingCoffee",
                table: "Reviews",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RatingPlace",
                table: "Reviews",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RatingService",
                table: "Reviews",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RatingCoffee",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingPlace",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "RatingService",
                table: "Reviews");

            migrationBuilder.CreateTable(
                name: "RatingCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewRatingCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RatingCategoryId = table.Column<int>(type: "integer", nullable: false),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewRatingCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewRatingCategories_RatingCategories_RatingCategoryId",
                        column: x => x.RatingCategoryId,
                        principalTable: "RatingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewRatingCategories_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRatingCategories_RatingCategoryId",
                table: "ReviewRatingCategories",
                column: "RatingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRatingCategories_ReviewId",
                table: "ReviewRatingCategories",
                column: "ReviewId");
        }
    }
}
