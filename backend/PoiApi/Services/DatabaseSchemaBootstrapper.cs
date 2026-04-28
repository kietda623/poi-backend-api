using Microsoft.EntityFrameworkCore;
using PoiApi.Data;

namespace PoiApi.Services;

public static class DatabaseSchemaBootstrapper
{
    public static async Task EnsureServicePackageSchemaAsync(AppDbContext context)
    {
        await EnsureColumnAsync(context, "ServicePackages", "Audience", "varchar(32) NOT NULL DEFAULT 'OWNER'");
        await EnsureColumnAsync(context, "ServicePackages", "AllowAudioAccess", "tinyint(1) NOT NULL DEFAULT 0");

        await EnsureColumnAsync(context, "Subscriptions", "PaymentProvider", "varchar(32) NOT NULL DEFAULT 'PayOS'");
        await EnsureColumnAsync(context, "Subscriptions", "PaymentStatus", "varchar(32) NOT NULL DEFAULT 'Pending'");
        await EnsureColumnAsync(context, "Subscriptions", "PaymentOrderCode", "bigint NULL");
        await EnsureColumnAsync(context, "Subscriptions", "PaymentLinkId", "varchar(255) NULL");
        await EnsureColumnAsync(context, "Subscriptions", "CheckoutUrl", "longtext NULL");
        await EnsureColumnAsync(context, "Subscriptions", "ActivatedAt", "datetime(6) NULL");
        await EnsureColumnAsync(context, "Subscriptions", "CancelAtPeriodEnd", "tinyint(1) NOT NULL DEFAULT 0");
        await EnsureColumnAsync(context, "Subscriptions", "CancelRequestedAt", "datetime(6) NULL");
        await EnsureColumnAsync(context, "Subscriptions", "RevenueRecipientUserId", "int NULL");
        await EnsureColumnAsync(context, "Subscriptions", "RevenueRecipientShopId", "int NULL");
    }

    public static async Task EnsureGuestFeatureSchemaAsync(AppDbContext context)
    {
        await EnsureSwipedItemsTableAsync(context);
        await EnsureColumnAsync(context, "SwipedItems", "DeviceId", "varchar(255) NULL");
        await EnsureNullableColumnAsync(context, "SwipedItems", "UserId", "int NULL");
        await EnsureIndexAsync(context, "SwipedItems", "IX_SwipedItems_DeviceId_ShopId", "CREATE UNIQUE INDEX `IX_SwipedItems_DeviceId_ShopId` ON `SwipedItems` (`DeviceId`, `ShopId`);");
    }

    private static async Task EnsureSwipedItemsTableAsync(AppDbContext context)
    {
        if (await TableExistsAsync(context, "SwipedItems"))
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync(@"
CREATE TABLE `SwipedItems` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` int NULL,
    `DeviceId` varchar(255) NULL,
    `ShopId` int NOT NULL,
    `IsLiked` tinyint(1) NOT NULL,
    `SwipedAt` datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    CONSTRAINT `PK_SwipedItems` PRIMARY KEY (`Id`)
);");

        await EnsureIndexAsync(context, "SwipedItems", "IX_SwipedItems_UserId_ShopId", "CREATE UNIQUE INDEX `IX_SwipedItems_UserId_ShopId` ON `SwipedItems` (`UserId`, `ShopId`);");
        await EnsureIndexAsync(context, "SwipedItems", "IX_SwipedItems_DeviceId_ShopId", "CREATE UNIQUE INDEX `IX_SwipedItems_DeviceId_ShopId` ON `SwipedItems` (`DeviceId`, `ShopId`);");
    }

    private static async Task EnsureColumnAsync(AppDbContext context, string tableName, string columnName, string definition)
    {
        if (await ColumnExistsAsync(context, tableName, columnName))
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {definition};");
    }

    private static async Task EnsureNullableColumnAsync(AppDbContext context, string tableName, string columnName, string definition)
    {
        if (!await ColumnExistsAsync(context, tableName, columnName))
        {
            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {definition};");
            return;
        }

        if (await IsColumnNullableAsync(context, tableName, columnName))
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` MODIFY COLUMN `{columnName}` {definition};");
    }

    private static async Task EnsureIndexAsync(AppDbContext context, string tableName, string indexName, string createSql)
    {
        var conn = await EnsureOpenConnectionAsync(context);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.STATISTICS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND INDEX_NAME = @indexName;";

        AddParameter(cmd, "@tableName", tableName);
        AddParameter(cmd, "@indexName", indexName);

        var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        if (!exists)
        {
            await context.Database.ExecuteSqlRawAsync(createSql);
        }
    }

    private static async Task<bool> TableExistsAsync(AppDbContext context, string tableName)
    {
        var conn = await EnsureOpenConnectionAsync(context);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName;";

        AddParameter(cmd, "@tableName", tableName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(AppDbContext context, string tableName, string columnName)
    {
        var conn = await EnsureOpenConnectionAsync(context);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND COLUMN_NAME = @columnName;";

        AddParameter(cmd, "@tableName", tableName);
        AddParameter(cmd, "@columnName", columnName);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
    }

    private static async Task<bool> IsColumnNullableAsync(AppDbContext context, string tableName, string columnName)
    {
        var conn = await EnsureOpenConnectionAsync(context);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
SELECT IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND COLUMN_NAME = @columnName
LIMIT 1;";

        AddParameter(cmd, "@tableName", tableName);
        AddParameter(cmd, "@columnName", columnName);
        var value = Convert.ToString(await cmd.ExecuteScalarAsync());
        return string.Equals(value, "YES", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<System.Data.Common.DbConnection> EnsureOpenConnectionAsync(AppDbContext context)
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        return conn;
    }

    private static void AddParameter(System.Data.Common.DbCommand cmd, string name, object value)
    {
        var parameter = cmd.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        cmd.Parameters.Add(parameter);
    }
}
