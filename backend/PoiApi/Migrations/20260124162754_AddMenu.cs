using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddMenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "POIs");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "POITranslations",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "POIs",
                newName: "Location");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "POITranslations",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "POIs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "POITranslations");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "POIs");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "POITranslations",
                newName: "Content");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "POIs",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "POIs",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
