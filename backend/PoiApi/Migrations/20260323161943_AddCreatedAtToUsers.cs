using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Shops",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PoiId",
                table: "Shops",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "POIId",
                table: "Menus",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Menus",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Menus",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Menus",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ShopId",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MenuItems",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MenuItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "MenuItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MenuItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "MenuItems",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Shops_PoiId",
                table: "Shops",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ShopId",
                table: "Menus",
                column: "ShopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_POIs_POIId",
                table: "Menus",
                column: "POIId",
                principalTable: "POIs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Shops_ShopId",
                table: "Menus",
                column: "ShopId",
                principalTable: "Shops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shops_POIs_PoiId",
                table: "Shops",
                column: "PoiId",
                principalTable: "POIs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_POIs_POIId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Shops_ShopId",
                table: "Menus");

            migrationBuilder.DropForeignKey(
                name: "FK_Shops_POIs_PoiId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Shops_PoiId",
                table: "Shops");

            migrationBuilder.DropIndex(
                name: "IX_Menus_ShopId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "PoiId",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "ShopId",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MenuItems");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "MenuItems");

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

            migrationBuilder.AlterColumn<int>(
                name: "poi_id",
                table: "Menus",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

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
    }
}
