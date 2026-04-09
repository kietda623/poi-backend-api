using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    public partial class AddPackageAudienceAndPayOsFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Audience",
                table: "ServicePackages",
                type: "longtext",
                nullable: false,
                defaultValue: "OWNER")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAudioAccess",
                table: "ServicePackages",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                table: "Subscriptions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckoutUrl",
                table: "Subscriptions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentLinkId",
                table: "Subscriptions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "PaymentOrderCode",
                table: "Subscriptions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentProvider",
                table: "Subscriptions",
                type: "longtext",
                nullable: false,
                defaultValue: "PayOS")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "Subscriptions",
                type: "longtext",
                nullable: false,
                defaultValue: "Pending")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Audience", "AllowAudioAccess", "Description", "Features", "Name" },
                values: new object[] { "OWNER", false, "Goi danh cho seller moi bat dau.", "Hien thi tren ban do|1 gian hang|Ho tro qua email", "Goi Co ban" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Audience", "AllowAudioAccess", "Description", "Features", "Name" },
                values: new object[] { "OWNER", false, "Goi seller co uu tien de xuat va thong ke nang cao.", "Uu tien de xuat|Badge Premium|3 gian hang|Ho tro uu tien|Thong ke nang cao", "Goi Nang cao" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Audience", "AllowAudioAccess", "Description", "Features" },
                values: new object[] { "OWNER", false, "Goi seller day du quyen loi va ho tro rieng.", "Top de xuat tren app|Badge VIP|5 gian hang|Ho tro rieng 24/7|Thong ke chi tiet|Quang cao banner" });

            migrationBuilder.InsertData(
                table: "ServicePackages",
                columns: new[] { "Id", "AllowAudioAccess", "Audience", "CreatedAt", "Description", "Features", "IsActive", "MaxStores", "MonthlyPrice", "Name", "Tier", "YearlyPrice" },
                values: new object[,]
                {
                    { 4, true, "USER", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Goi nghe thuyet minh co ban danh cho nguoi dung app.", "Nghe thuyet minh 3 ngon ngu|Ho tro review sau khi nghe|Su dung tren toan bo app", true, 0, 49000m, "Audio Starter", "Basic", 490000m },
                    { 5, true, "USER", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Goi audio uu tien danh cho nguoi nghe thuong xuyen.", "Nghe thuyet minh 3 ngon ngu|Uu tien audio moi|Khong gioi han luot nghe trong goi con han", true, 0, 99000m, "Audio Plus", "Premium", 990000m },
                    { 6, true, "USER", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Goi audio cao cap cho nguoi dung trung thanh.", "Nghe thuyet minh 3 ngon ngu|Truy cap tat ca diem audio|Ho tro uu tien", true, 0, 199000m, "Audio Premium", "VIP", 1990000m }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "ServicePackages", keyColumn: "Id", keyValue: 4);
            migrationBuilder.DeleteData(table: "ServicePackages", keyColumn: "Id", keyValue: 5);
            migrationBuilder.DeleteData(table: "ServicePackages", keyColumn: "Id", keyValue: 6);

            migrationBuilder.DropColumn(name: "Audience", table: "ServicePackages");
            migrationBuilder.DropColumn(name: "AllowAudioAccess", table: "ServicePackages");
            migrationBuilder.DropColumn(name: "ActivatedAt", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "CheckoutUrl", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "PaymentLinkId", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "PaymentOrderCode", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "PaymentProvider", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "PaymentStatus", table: "Subscriptions");
        }
    }
}
