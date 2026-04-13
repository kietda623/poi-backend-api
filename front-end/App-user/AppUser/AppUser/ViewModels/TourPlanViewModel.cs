using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels;

public partial class TourPlanViewModel : ObservableObject
{
    private readonly AiService _aiService;
    private bool _hasInitialized;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isAccessDenied;

    [ObservableProperty]
    private string preferences = string.Empty;

    [ObservableProperty]
    private string tourPlanResult = string.Empty;

    [ObservableProperty]
    private bool hasResult;

    [ObservableProperty]
    private int likedCount;

    [ObservableProperty]
    private ObservableCollection<TinderCardDto> cards = new();

    [ObservableProperty]
    private int remainingCount;

    public TourPlanViewModel(AiService aiService)
    {
        _aiService = aiService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsLoading || _hasInitialized) return;
        IsLoading = true;

        try
        {
            var info = await _aiService.GetSubscriptionInfoAsync();
            if (info == null || !info.AllowTinder || !info.AllowAiPlan)
            {
                IsAccessDenied = true;
                return;
            }

            IsAccessDenied = false;
            await RefreshLikedCountAsync();
            await LoadCardsAsync();
            _hasInitialized = true;
        }
        catch (Exception)
        {
            IsAccessDenied = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task GeneratePlanAsync()
    {
        if (IsLoading) return;
        
        IsLoading = true;
        HasResult = false;
        TourPlanResult = string.Empty;

        try
        {
            // 1. Get Liked Shop IDs from backend
            var likedResponse = await _aiService.GetLikedShopsAsync();
            if (likedResponse == null || !likedResponse.Success || likedResponse.Count == 0)
            {
                await Shell.Current.DisplayAlert("Thiếu dữ liệu", "Bạn cần quẹt 'Thích' ít nhất 1 quán ăn trong mục Tinder để AI có thể lập lịch trình.", "OK");
                return;
            }

            var shopIds = likedResponse.Shops.Select(s => s.ShopId).ToList();

            // 2. Call AI generate endpoint
            var planResponse = await _aiService.GenerateTourPlanAsync(new AiTourPlanRequestDto
            {
                LikedShopIds = shopIds,
                Preferences = Preferences
            });

            if (planResponse != null && planResponse.Success)
            {
                TourPlanResult = planResponse.TourPlan ?? "Không có phản hồi từ AI.";
                HasResult = true;
                
                // Send message to show popup
                WeakReferenceMessenger.Default.Send(new TourPlanGeneratedMessage(TourPlanResult));
            }
            else
            {
                await Shell.Current.DisplayAlert("Lỗi", "Không thể tạo lịch trình lúc này. Vui lòng thử lại sau.", "OK");
            }
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Đã có lỗi xảy ra.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task UpgradeAsync()
    {
        await Shell.Current.GoToAsync("subscriptionPackages");
    }

    [RelayCommand]
    public async Task LoadCardsAsync()
    {
        try
        {
            var response = await _aiService.GetTinderCardsAsync(10);
            if (response == null || !response.Success)
            {
                return;
            }

            Cards.Clear();
            foreach (var card in response.Cards)
            {
                Cards.Add(card);
            }

            RemainingCount = response.RemainingCount;
        }
        catch (Exception)
        {
        }
    }

    [RelayCommand]
    public async Task SwipeLeftAsync(TinderCardDto? card)
    {
        if (card == null) return;
        await _aiService.SwipeAsync(card.Id, false);
        RemoveCard(card);
    }

    [RelayCommand]
    public async Task SwipeRightAsync(TinderCardDto? card)
    {
        if (card == null) return;
        await _aiService.SwipeAsync(card.Id, true);
        LikedCount++;
        RemoveCard(card);
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        _hasInitialized = false;
        Cards.Clear();
        RemainingCount = 0;
        await InitializeAsync();
    }

    private async Task RefreshLikedCountAsync()
    {
        var likedResponse = await _aiService.GetLikedShopsAsync();
        LikedCount = likedResponse?.Success == true ? likedResponse.Count : 0;
    }

    private void RemoveCard(TinderCardDto card)
    {
        Cards.Remove(card);
        if (Cards.Count == 0)
        {
            _ = LoadCardsAsync();
        }
    }
}
