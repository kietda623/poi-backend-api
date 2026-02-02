using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class FixPoiRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_POIs_POIId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Username",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "POIId",
                table: "Menus",
                newName: "poi_id");

            migrationBuilder.RenameIndex(
                name: "IX_Menus_POIId",
                table: "Menus",
                newName: "IX_Menus_poi_id");

            migrationBuilder.AddColumn<int>(
                name: "PoiId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_PoiId",
                table: "Users",
                column: "PoiId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_POIs_poi_id",
                table: "Menus",
                column: "poi_id",
                principalTable: "POIs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_POIs_PoiId",
                table: "Users",
                column: "PoiId",
                principalTable: "POIs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_POIs_poi_id",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_POIs_PoiId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PoiId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PoiId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "poi_id",
                table: "Menus",
                newName: "POIId");

            migrationBuilder.RenameIndex(
                name: "IX_Menus_poi_id",
                table: "Menus",
                newName: "IX_Menus_POIId");

            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_POIs_POIId",
                table: "Menus",
                column: "POIId",
                principalTable: "POIs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
