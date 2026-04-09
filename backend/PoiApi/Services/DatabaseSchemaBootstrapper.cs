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

    private static async Task EnsureColumnAsync(AppDbContext context, string tableName, string columnName, string definition)
    {
        var conn = context.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
        {
            await conn.OpenAsync();
        }

        await using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = @"
SELECT COUNT(*)
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME = @tableName
  AND COLUMN_NAME = @columnName;";

        var tableParam = checkCmd.CreateParameter();
        tableParam.ParameterName = "@tableName";
        tableParam.Value = tableName;
        checkCmd.Parameters.Add(tableParam);

        var columnParam = checkCmd.CreateParameter();
        columnParam.ParameterName = "@columnName";
        columnParam.Value = columnName;
        checkCmd.Parameters.Add(columnParam);

        var exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
        if (exists)
        {
            return;
        }

        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE `{tableName}` ADD COLUMN `{columnName}` {definition};");
    }
}
