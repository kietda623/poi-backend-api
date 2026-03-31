using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class ServicePackageService
{
    private readonly ApiService _api;

    public ServicePackageService(ApiService api)
    {
        _api = api;
    }

    // ── Packages CRUD ───────────────────────────────────────────────

    public async Task<List<ServicePackageModel>> GetPackagesAsync()
    {
        return await _api.GetAsync<List<ServicePackageModel>>("admin/service-packages") ?? new();
    }

    public async Task<(bool Ok, string Msg)> CreatePackageAsync(ServicePackageModel m)
    {
        // Backend mong đợi Features dạng chuỗi tách bằng |
        var dto = new
        {
            m.Name,
            m.Tier,
            m.MonthlyPrice,
            m.YearlyPrice,
            m.Description,
            Features = string.Join("|", m.Features),
            m.MaxStores,
            m.IsActive
        };

        var response = await _api.PostAsync<object, dynamic>("admin/service-packages", dto);
        if (response != null)
            return (true, "Tạo gói dịch vụ thành công!");
        return (false, "Lỗi khi tạo gói!");
    }

    public async Task<(bool Ok, string Msg)> UpdatePackageAsync(ServicePackageModel m)
    {
        var dto = new
        {
            m.Name,
            m.Tier,
            m.MonthlyPrice,
            m.YearlyPrice,
            m.Description,
            Features = string.Join("|", m.Features),
            m.MaxStores,
            m.IsActive
        };

        var response = await _api.PutAsync<object, dynamic>($"admin/service-packages/{m.Id}", dto);
        if (response != null)
            return (true, "Cập nhật gói thành công!");
        return (false, "Lỗi khi cập nhật!");
    }

    public async Task<bool> DeletePackageAsync(int id)
    {
        return await _api.DeleteAsync($"admin/service-packages/{id}");
    }

    // ── Subscriptions ───────────────────────────────────────────────

    public async Task<List<SubscriptionModel>> GetSubscriptionsAsync()
    {
        return await _api.GetAsync<List<SubscriptionModel>>("admin/service-packages/subscriptions") ?? new();
    }

    public async Task<bool> ApproveSubscriptionAsync(int id)
    {
        return await _api.PatchAsync($"admin/service-packages/subscriptions/{id}/approve");
    }

    public async Task<bool> CancelSubscriptionAsync(int id)
    {
        return await _api.PatchAsync($"admin/service-packages/subscriptions/{id}/cancel");
    }
}
