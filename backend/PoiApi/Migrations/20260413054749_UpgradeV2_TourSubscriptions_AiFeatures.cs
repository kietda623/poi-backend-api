using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PoiApi.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeV2_TourSubscriptions_AiFeatures : Migration
    {
        /// <summary>
        /// MySQL-safe conditional ADD COLUMN using stored procedure.
        /// Creates a temp procedure, calls it, then drops it.
        /// </summary>
        private void AddColumnIfNotExists(MigrationBuilder mb, string table, string column, string colDef)
        {
            mb.Sql($"DROP PROCEDURE IF EXISTS _AddCol_{table}_{column};");
            mb.Sql($@"
CREATE PROCEDURE _AddCol_{table}_{column}()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{table}' AND COLUMN_NAME = '{column}'
    ) THEN
        ALTER TABLE `{table}` ADD COLUMN `{column}` {colDef};
    END IF;
END;");
            mb.Sql($"CALL _AddCol_{table}_{column}();");
            mb.Sql($"DROP PROCEDURE IF EXISTS _AddCol_{table}_{column};");
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ===== CONDITIONAL ADD COLUMNS (may already exist from prev manual migration) =====

            // Subscriptions table - PayOS fields
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "ActivatedAt", "datetime(6) NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "CancelAtPeriodEnd", "tinyint(1) NOT NULL DEFAULT 0");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "CancelRequestedAt", "datetime(6) NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "CheckoutUrl", "longtext NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "PaymentLinkId", "longtext NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "PaymentOrderCode", "bigint NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "PaymentProvider", "longtext NOT NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "PaymentStatus", "longtext NOT NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "RevenueRecipientShopId", "int NULL");
            AddColumnIfNotExists(migrationBuilder, "Subscriptions", "RevenueRecipientUserId", "int NULL");

            // Shops table
            AddColumnIfNotExists(migrationBuilder, "Shops", "AverageRating", "double NOT NULL DEFAULT 0");

            // ServicePackages table
            AddColumnIfNotExists(migrationBuilder, "ServicePackages", "AllowAudioAccess", "tinyint(1) NOT NULL DEFAULT 0");
            AddColumnIfNotExists(migrationBuilder, "ServicePackages", "Audience", "longtext NOT NULL");

            // V2 new columns
            AddColumnIfNotExists(migrationBuilder, "ServicePackages", "AllowAiPlanAccess", "tinyint(1) NOT NULL DEFAULT 0");
            AddColumnIfNotExists(migrationBuilder, "ServicePackages", "AllowChatbotAccess", "tinyint(1) NOT NULL DEFAULT 0");
            AddColumnIfNotExists(migrationBuilder, "ServicePackages", "AllowTinderAccess", "tinyint(1) NOT NULL DEFAULT 0");

            // ===== CREATE TABLE SwipedItems =====
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS `SwipedItems` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NOT NULL,
    `ShopId` int NOT NULL,
    `IsLiked` tinyint(1) NOT NULL,
    `SwipedAt` datetime(6) NOT NULL,
    PRIMARY KEY (`Id`),
    KEY `IX_SwipedItems_ShopId` (`ShopId`),
    UNIQUE KEY `IX_SwipedItems_UserId_ShopId` (`UserId`, `ShopId`),
    CONSTRAINT `FK_SwipedItems_Shops_ShopId` FOREIGN KEY (`ShopId`) REFERENCES `Shops` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_SwipedItems_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;");

            // ===== UPDATE SEED DATA =====
            // Update Owner packages (Id 1-3)
            migrationBuilder.Sql(@"UPDATE `ServicePackages` SET
    `AllowAiPlanAccess` = 0, `AllowAudioAccess` = 0, `AllowChatbotAccess` = 0, `AllowTinderAccess` = 0,
    `Audience` = 'OWNER',
    `Name` = 'Basic',
    `Description` = 'Goi khoi dau cho shop kinh doanh nho.',
    `Features` = 'Hien thi tren ban do|Toi da 1 gian hang|1 Menu, khong gioi han mon|Ho tro qua email'
WHERE `Id` = 1;");

            migrationBuilder.Sql(@"UPDATE `ServicePackages` SET
    `AllowAiPlanAccess` = 0, `AllowAudioAccess` = 0, `AllowChatbotAccess` = 0, `AllowTinderAccess` = 0,
    `Audience` = 'OWNER',
    `Name` = 'Premium',
    `Description` = 'Danh cho shop muon tang do nhan dien va hieu qua.',
    `Features` = 'Tat ca tinh nang Basic|Toi da 3 gian hang|Badge Premium tren app|Uu tien de xuat (score +50)|Thong ke nang cao|Ho tro uu tien'
WHERE `Id` = 2;");

            migrationBuilder.Sql(@"UPDATE `ServicePackages` SET
    `AllowAiPlanAccess` = 0, `AllowAudioAccess` = 0, `AllowChatbotAccess` = 0, `AllowTinderAccess` = 0,
    `Audience` = 'OWNER',
    `Name` = 'VIP',
    `Description` = 'Dac quyen cao cap nhat cho doanh nghiep lon.',
    `Features` = 'Tat ca tinh nang Premium|Toi da 5 gian hang|Badge VIP tren app|Top de xuat (score +100)|Thong ke chi tiet|Quang cao tren banner|Ho tro rieng 24/7'
WHERE `Id` = 3;");

            // Deactivate old User packages (4-6) if they exist
            migrationBuilder.Sql(@"UPDATE `ServicePackages` SET `IsActive` = 0,
    `AllowAiPlanAccess` = 0, `AllowChatbotAccess` = 0, `AllowTinderAccess` = 0
WHERE `Id` IN (4, 5, 6);");

            // Insert Tour Basic (Id 7) + Tour Plus (Id 8)
            migrationBuilder.Sql(@"INSERT IGNORE INTO `ServicePackages`
    (`Id`, `AllowAiPlanAccess`, `AllowAudioAccess`, `AllowChatbotAccess`, `AllowTinderAccess`, `Audience`, `CreatedAt`, `Description`, `Features`, `IsActive`, `MaxStores`, `MonthlyPrice`, `Name`, `Tier`, `YearlyPrice`)
VALUES
    (7, 0, 1, 0, 0, 'USER', '2026-01-01 00:00:00', 'Mo khoa thuyet minh am thuc tu dong khi den gan cac gian hang.', 'Tu dong phat thuyet minh khi den gan POI|Nghe thuyet minh 3 ngon ngu|Ho tro review sau khi nghe', 1, 0, 99000, 'Tour Basic', 'Basic', 990000),
    (8, 1, 1, 1, 1, 'USER', '2026-01-01 00:00:00', 'Trai nghiem day du: thuyet minh + Tinder am thuc + AI lich trinh + Chatbot tu van.', 'Tat ca quyen loi Tour Basic|Tinder Am Thuc (quet trai/phai)|AI Ke Hoach Tour tu Gemini|Chatbot Tho Dia tu van mon an|Uu tien de xuat quan hot', 1, 0, 299000, 'Tour Plus', 'Premium', 2990000);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `SwipedItems`;");

            migrationBuilder.Sql("DELETE FROM `ServicePackages` WHERE `Id` IN (7, 8);");
            migrationBuilder.Sql("UPDATE `ServicePackages` SET `IsActive` = 1 WHERE `Id` IN (4, 5, 6);");

            migrationBuilder.DropColumn(name: "AllowAiPlanAccess", table: "ServicePackages");
            migrationBuilder.DropColumn(name: "AllowChatbotAccess", table: "ServicePackages");
            migrationBuilder.DropColumn(name: "AllowTinderAccess", table: "ServicePackages");
            migrationBuilder.DropColumn(name: "AverageRating", table: "Shops");
            migrationBuilder.DropColumn(name: "CancelAtPeriodEnd", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "CancelRequestedAt", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "RevenueRecipientShopId", table: "Subscriptions");
            migrationBuilder.DropColumn(name: "RevenueRecipientUserId", table: "Subscriptions");
        }
    }
}
