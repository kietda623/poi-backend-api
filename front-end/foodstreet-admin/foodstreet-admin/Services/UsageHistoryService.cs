using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class UsageHistoryService
{
    private static List<UsageHistoryModel> _mockHistory = new()
    {
        new() { Id=1, DeviceId="Device-A1", StoreId=1, StoreName="Quán Ốc Bà Ba", LanguageCode="vi", ListenedAt=DateTime.Now.AddMinutes(-10), DurationSeconds=45 },
        new() { Id=2, DeviceId="User-B2", StoreId=4, StoreName="Bún Bò Huế Cô Ba", LanguageCode="en", ListenedAt=DateTime.Now.AddHours(-2), DurationSeconds=120 },
        new() { Id=3, DeviceId="Device-C3", StoreId=1, StoreName="Quán Ốc Bà Ba", LanguageCode="vi", ListenedAt=DateTime.Now.AddDays(-1), DurationSeconds=30 }
    };

    public async Task<List<UsageHistoryModel>> GetUsageHistoriesAsync()
    {
        await Task.Delay(100);
        return _mockHistory.OrderByDescending(x => x.ListenedAt).ToList();
    }

    public async Task RecordHistoryAsync(UsageHistoryModel model)
    {
        await Task.Delay(50);
        model.Id = _mockHistory.Count > 0 ? _mockHistory.Max(x => x.Id) + 1 : 1;
        _mockHistory.Add(model);
    }
}
