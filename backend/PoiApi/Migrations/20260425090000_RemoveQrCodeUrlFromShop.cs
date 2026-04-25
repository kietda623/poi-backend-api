using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using PoiApi.Data;

#nullable disable

namespace PoiApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260425090000_RemoveQrCodeUrlFromShop")]
    public partial class RemoveQrCodeUrlFromShop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QrCodeUrl",
                table: "Shops");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QrCodeUrl",
                table: "Shops",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
