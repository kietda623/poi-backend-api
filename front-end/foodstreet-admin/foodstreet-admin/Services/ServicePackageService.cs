using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class ServicePackageService
{
    private readonly ApiService _api;

    public ServicePackageService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<ServicePackageModel>> GetPackagesAsync()
    {
        return await _api.GetAsync<List<ServicePackageModel>>("admin/service-packages") ?? new();
    }

    public async Task<(bool Ok, string Msg)> CreatePackageAsync(ServicePackageModel model)
    {
        var dto = new
        {
            model.Name,
            model.Tier,
            model.Audience,
            model.MonthlyPrice,
            model.YearlyPrice,
            model.Description,
            Features = string.Join("|", model.Features),
            model.MaxStores,
            model.AllowAudioAccess,
            model.IsActive
        };

        var response = await _api.PostAsync<object, dynamic>("admin/service-packages", dto);
        return response != null
            ? (true, "Tao goi dich vu thanh cong.")
            : (false, "Khong the tao goi dich vu.");
    }

    public async Task<(bool Ok, string Msg)> UpdatePackageAsync(ServicePackageModel model)
    {
        var dto = new
        {
            model.Name,
            model.Tier,
            model.Audience,
            model.MonthlyPrice,
            model.YearlyPrice,
            model.Description,
            Features = string.Join("|", model.Features),
            model.MaxStores,
            model.AllowAudioAccess,
            model.IsActive
        };

        var response = await _api.PutAsync<object, dynamic>($"admin/service-packages/{model.Id}", dto);
        return response != null
            ? (true, "Cap nhat goi dich vu thanh cong.")
            : (false, "Khong the cap nhat goi dich vu.");
    }

    public Task<bool> DeletePackageAsync(int id)
    {
        return _api.DeleteAsync($"admin/service-packages/{id}");
    }

    public async Task<List<SubscriptionModel>> GetSubscriptionsAsync()
    {
        return await _api.GetAsync<List<SubscriptionModel>>("admin/service-packages/subscriptions") ?? new();
    }

    public Task<bool> ApproveSubscriptionAsync(int id)
    {
        return _api.PatchAsync($"admin/service-packages/subscriptions/{id}/approve");
    }

    public Task<bool> CancelSubscriptionAsync(int id)
    {
        return _api.PatchAsync($"admin/service-packages/subscriptions/{id}/cancel");
    }

    public async Task<List<ServicePackageModel>> GetOwnerPackagesAsync()
    {
        return await _api.GetAsync<List<ServicePackageModel>>("owner/subscriptions/packages") ?? new();
    }

    public async Task<CurrentSubscriptionEnvelopeModel> GetOwnerCurrentSubscriptionAsync()
    {
        return await _api.GetAsync<CurrentSubscriptionEnvelopeModel>("owner/subscriptions/my") ?? new CurrentSubscriptionEnvelopeModel();
    }

    public async Task<List<CurrentSubscriptionModel>> GetOwnerSubscriptionHistoryAsync()
    {
        return await _api.GetAsync<List<CurrentSubscriptionModel>>("owner/subscriptions/history") ?? new();
    }

    public async Task<CheckoutSubscriptionResultModel?> CreateOwnerSubscriptionCheckoutAsync(int packageId, string billingCycle)
    {
        return await _api.PostAsync<object, CheckoutSubscriptionResultModel>("owner/subscriptions", new
        {
            packageId,
            billingCycle
        });
    }

    public async Task<CurrentSubscriptionEnvelopeModel> SyncOwnerSubscriptionPaymentAsync(int subscriptionId)
    {
        return await _api.PostAsync<object, CurrentSubscriptionEnvelopeModel>($"owner/subscriptions/{subscriptionId}/sync-payment", new { })
            ?? new CurrentSubscriptionEnvelopeModel();
    }

    public Task<bool> CancelOwnerSubscriptionAsync(int id)
    {
        return _api.DeleteAsync($"owner/subscriptions/{id}");
    }
}
