using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestAccessSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions");

            migrationBuilder.AddColumn<string>(
                name: "GuestId",
                table: "UsageHistories",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Subscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                table: "Subscriptions",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GuestEmail",
                table: "Subscriptions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Features", "Tier" },
                values: new object[] { "Gói nghe thuyết minh cơ bản.", "Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe", "AudioBasic" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Features", "Tier" },
                values: new object[] { "Gói nghe thuyết minh mở rộng.", "Nghe thuyết minh 3 ngôn ngữ|Ưu tiên audio mới", "AudioPremium" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Features", "Tier" },
                values: new object[] { "Gói audio cao cấp.", "Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ ưu tiên", "AudioVIP" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Features", "MonthlyPrice", "Tier", "YearlyPrice" },
                values: new object[] { "Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng. Sử dụng trong 1 ngày.", "Sử dụng trong 1 ngày|Tự động phát thuyết minh khi đến gần POI|Nghe thuyết minh 3 ngôn ngữ|Hỗ trợ review sau khi nghe|!Tinder Ẩm Thực|!AI Kế Hoạch Tour|!Chatbot Thổ Địa", 50000m, "TourBasic", 50000m });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Features", "MonthlyPrice", "Tier", "YearlyPrice" },
                values: new object[] { "Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn. Sử dụng trong 1 ngày.", "Sử dụng trong 1 ngày|Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Groq|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot", 99000m, "TourPlus", 99000m });

            migrationBuilder.CreateIndex(
                name: "IX_UsageHistories_GuestId",
                table: "UsageHistories",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_DeviceId",
                table: "Subscriptions",
                column: "DeviceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UsageHistories_GuestId",
                table: "UsageHistories");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_DeviceId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "GuestId",
                table: "UsageHistories");

            migrationBuilder.DropColumn(
                name: "DeviceId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "GuestEmail",
                table: "Subscriptions");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Description", "Features", "Tier" },
                values: new object[] { "Goi nghe thuyet minh co ban danh cho nguoi dung app.", "Nghe thuyet minh 3 ngon ngu|Ho tro review sau khi nghe|Su dung tren toan bo app", "Basic" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Description", "Features", "Tier" },
                values: new object[] { "Goi audio uu tien danh cho nguoi nghe thuong xuyen.", "Nghe thuyet minh 3 ngon ngu|Uu tien audio moi|Khong gioi han luot nghe trong goi con han", "Premium" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Description", "Features", "Tier" },
                values: new object[] { "Goi audio cao cap cho nguoi dung trung thanh.", "Nghe thuyet minh 3 ngon ngu|Truy cap tat ca diem audio|Ho tro uu tien", "VIP" });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Description", "Features", "MonthlyPrice", "Tier", "YearlyPrice" },
                values: new object[] { "Mở khóa thuyết minh ẩm thực tự động khi đến gần các gian hàng.", "Tự động phát thuyết minh khi đến gần POI|Nghe thuyet minh 3 ngon ngu|Ho tro review sau khi nghe|!Tinder Ẩm Thực|!AI Kế Hoạch Tour|!Chatbot Thổ Địa", 99000m, "Basic", 990000m });

            migrationBuilder.UpdateData(
                table: "ServicePackages",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Description", "Features", "MonthlyPrice", "Tier", "YearlyPrice" },
                values: new object[] { "Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot tư vấn.", "Tất cả quyền lợi Tour Basic|Tinder Ẩm Thực (quẹt trái/phải)|AI Kế Hoạch Tour từ Gemini|Chatbot Thổ Địa tư vấn món ăn|Ưu tiên đề xuất quán hot", 299000m, "Premium", 2990000m });

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
