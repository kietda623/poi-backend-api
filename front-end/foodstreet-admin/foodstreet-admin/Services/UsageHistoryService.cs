using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class UsageHistoryService
{
    private readonly ApiService _api;

    public UsageHistoryService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<UsageHistoryModel>> GetUsageHistoriesAsync()
    {
        return await _api.GetAsync<List<UsageHistoryModel>>("admin/usage-history") ?? new();
    }

    public async Task RecordHistoryAsync(UsageHistoryModel model)
    {
        // Future extension: Call api/admin/usage-history POST to record
        await _api.PostAsync<UsageHistoryModel, object>("admin/usage-history", model);
    }
}
