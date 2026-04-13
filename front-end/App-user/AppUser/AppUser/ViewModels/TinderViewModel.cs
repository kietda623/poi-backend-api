using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels;

public partial class TinderViewModel : ObservableObject
{
    private readonly AiService _aiService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isAccessDenied;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TinderCardDto> cards = new();

    [ObservableProperty]
    private int remainingCount;

    public TinderViewModel(AiService aiService)
    {
        _aiService = aiService;
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var info = await _aiService.GetSubscriptionInfoAsync();
            if (info == null || !info.AllowTinder)
            {
                Console.WriteLine($"Tinder Access Denied: info is null? {info == null}, AllowTinder? {info?.AllowTinder}");
                IsAccessDenied = true;
                return;
            }
            Console.WriteLine("Tinder Access Granted!");

            IsAccessDenied = false;
            if (Cards.Count == 0)
            {
                await LoadCardsAsync();
            }
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
    public async Task LoadCardsAsync()
    {
        try
        {
            var response = await _aiService.GetTinderCardsAsync(10);
            if (response != null && response.Success)
            {
                Cards.Clear();
                foreach (var card in response.Cards)
                {
                    Cards.Add(card);
                }
                RemainingCount = response.RemainingCount;
            }
        }
        catch (Exception)
        {
            StatusMessage = "Không thể tải danh sách món ăn.";
        }
    }

    [RelayCommand]
    public async Task SwipeLeft(TinderCardDto card)
    {
        if (card == null) return;
        await _aiService.SwipeAsync(card.Id, false);
        RemoveCard(card);
    }

    [RelayCommand]
    public async Task SwipeRight(TinderCardDto card)
    {
        if (card == null) return;
        await _aiService.SwipeAsync(card.Id, true);
        RemoveCard(card);
    }

    private void RemoveCard(TinderCardDto card)
    {
        Cards.Remove(card);
        if (Cards.Count == 0)
        {
            _ = LoadCardsAsync();
        }
    }

    [RelayCommand]
    public async Task UpgradeAsync()
    {
        await Shell.Current.GoToAsync("subscriptionPackages");
    }
}
