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
    private readonly AudioService _audioService;
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

    // Localized UI strings
    [ObservableProperty]
    private string pageTitle = "Tour Plan";

    [ObservableProperty]
    private string headerTitle = "Tour Plan Groq";

    [ObservableProperty]
    private string headerSubtitle = "Lên lịch trình ẩm thực thông minh trong chớp mắt";

    [ObservableProperty]
    private string step1Label = "Chọn quán";

    [ObservableProperty]
    private string step2Label = "Sở thích";

    [ObservableProperty]
    private string step3Label = "Nhận Tour";

    [ObservableProperty]
    private string suggestTitle = "Gợi ý món ngon";

    [ObservableProperty]
    private string likedCountText = "Đã thích 0 quán";

    [ObservableProperty]
    private string emptyCardText = "Đã hết quán để quẹt hôm nay";

    [ObservableProperty]
    private string loadMoreText = "Tải thêm quán";

    [ObservableProperty]
    private string skipText = "Bỏ qua";

    [ObservableProperty]
    private string likeText = "Thích";

    [ObservableProperty]
    private string personalizeTitle = "Cá nhân hóa lịch trình";

    [ObservableProperty]
    private string personalizeHint = "Thêm ghi chú về thời gian, ngân sách hoặc yêu cầu đặc biệt:";

    [ObservableProperty]
    private string preferencesPlaceholder = "Ví dụ: Đi buổi tối, ngân sách 500k, thích không gian yên tĩnh...";

    [ObservableProperty]
    private string generateButtonText = "Tạo Tour AI ngay";

    [ObservableProperty]
    private string loadingText = "AI đang 'may đo' lịch trình dành riêng cho bạn...";

    [ObservableProperty]
    private string accessDeniedTitle = "Nâng tầm trải nghiệm";

    [ObservableProperty]
    private string accessDeniedDescription = "Mở khóa tính năng lập lịch trình AI không giới hạn với gói Tour Plus.";

    [ObservableProperty]
    private string upgradeButtonText = "Khám phá ngay";

    public TourPlanViewModel(AiService aiService, AudioService audioService)
    {
        _aiService = aiService;
        _audioService = audioService;
        _audioService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedTexts();
    }

    partial void OnLikedCountChanged(int value)
    {
        UpdateLikedCountText();
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
                await Shell.Current.DisplayAlert(
                    GetText("alert_missing_data_title"),
                    GetText("alert_missing_data_body"),
                    "OK");
                return;
            }

            var shopIds = likedResponse.Shops.Select(s => s.ShopId).ToList();

            // 2. Call AI generate endpoint with language
            var planResponse = await _aiService.GenerateTourPlanAsync(new AiTourPlanRequestDto
            {
                LikedShopIds = shopIds,
                Preferences = Preferences,
                Language = _audioService.CurrentLanguage
            });

            if (planResponse != null && planResponse.Success)
            {
                TourPlanResult = planResponse.TourPlan ?? GetText("no_ai_response");
                HasResult = true;
                
                // Send message to show popup
                WeakReferenceMessenger.Default.Send(new TourPlanGeneratedMessage(TourPlanResult));
            }
            else
            {
                await Shell.Current.DisplayAlert(
                    GetText("alert_error_title"),
                    GetText("alert_error_body"),
                    "OK");
            }
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert(
                GetText("alert_error_title"),
                GetText("alert_generic_error"),
                "OK");
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

    private void OnLanguageChanged(object? sender, string language)
    {
        UpdateLocalizedTexts();
    }

    private void UpdateLikedCountText()
    {
        var lang = _audioService.CurrentLanguage;
        LikedCountText = lang switch
        {
            "en" => $"Liked {LikedCount} restaurants",
            "zh" => $"已喜欢 {LikedCount} 家餐厅",
            _ => $"Đã thích {LikedCount} quán"
        };
    }

    private void UpdateLocalizedTexts()
    {
        PageTitle = GetText("page_title");
        HeaderTitle = GetText("header_title");
        HeaderSubtitle = GetText("header_subtitle");
        Step1Label = GetText("step1");
        Step2Label = GetText("step2");
        Step3Label = GetText("step3");
        SuggestTitle = GetText("suggest_title");
        UpdateLikedCountText();
        EmptyCardText = GetText("empty_card");
        LoadMoreText = GetText("load_more");
        SkipText = GetText("skip");
        LikeText = GetText("like");
        PersonalizeTitle = GetText("personalize_title");
        PersonalizeHint = GetText("personalize_hint");
        PreferencesPlaceholder = GetText("preferences_placeholder");
        GenerateButtonText = GetText("generate_button");
        LoadingText = GetText("loading");
        AccessDeniedTitle = GetText("access_denied_title");
        AccessDeniedDescription = GetText("access_denied_description");
        UpgradeButtonText = GetText("upgrade_button");
    }

    private string GetText(string key)
    {
        var lang = _audioService.CurrentLanguage;
        return (lang, key) switch
        {
            ("en", "page_title") => "Tour Plan",
            ("en", "header_title") => "Tour Plan Groq",
            ("en", "header_subtitle") => "Create a smart food itinerary in a flash",
            ("en", "step1") => "Pick shops",
            ("en", "step2") => "Preferences",
            ("en", "step3") => "Get Tour",
            ("en", "suggest_title") => "Food suggestions",
            ("en", "empty_card") => "No more shops to swipe today",
            ("en", "load_more") => "Load more shops",
            ("en", "skip") => "Skip",
            ("en", "like") => "Like",
            ("en", "personalize_title") => "Personalize your itinerary",
            ("en", "personalize_hint") => "Add notes about time, budget or special requests:",
            ("en", "preferences_placeholder") => "E.g.: Evening tour, budget 500k VND, prefer quiet places...",
            ("en", "generate_button") => "Generate AI Tour now",
            ("en", "loading") => "AI is crafting a personalized itinerary just for you...",
            ("en", "access_denied_title") => "Elevate your experience",
            ("en", "access_denied_description") => "Unlock unlimited AI itinerary planning with the Tour Plus package.",
            ("en", "upgrade_button") => "Explore now",
            ("en", "alert_missing_data_title") => "Missing data",
            ("en", "alert_missing_data_body") => "You need to 'Like' at least 1 restaurant in Tinder so that AI can create an itinerary.",
            ("en", "alert_error_title") => "Error",
            ("en", "alert_error_body") => "Unable to create itinerary at this time. Please try again later.",
            ("en", "alert_generic_error") => "An error occurred.",
            ("en", "no_ai_response") => "No response from AI.",

            ("zh", "page_title") => "旅行计划",
            ("zh", "header_title") => "Tour Plan Groq",
            ("zh", "header_subtitle") => "即刻创建智能美食行程",
            ("zh", "step1") => "选择餐厅",
            ("zh", "step2") => "偏好",
            ("zh", "step3") => "获取行程",
            ("zh", "suggest_title") => "美食推荐",
            ("zh", "empty_card") => "今天没有更多餐厅可以浏览了",
            ("zh", "load_more") => "加载更多餐厅",
            ("zh", "skip") => "跳过",
            ("zh", "like") => "喜欢",
            ("zh", "personalize_title") => "个性化行程",
            ("zh", "personalize_hint") => "添加关于时间、预算或特殊要求的备注：",
            ("zh", "preferences_placeholder") => "例如：晚上出行，预算50万越南盾，喜欢安静的环境...",
            ("zh", "generate_button") => "立即生成AI行程",
            ("zh", "loading") => "AI正在为您量身定制专属行程...",
            ("zh", "access_denied_title") => "提升体验",
            ("zh", "access_denied_description") => "使用Tour Plus套餐解锁无限AI行程规划。",
            ("zh", "upgrade_button") => "立即探索",
            ("zh", "alert_missing_data_title") => "缺少数据",
            ("zh", "alert_missing_data_body") => "您需要在Tinder中\"喜欢\"至少1家餐厅，AI才能创建行程。",
            ("zh", "alert_error_title") => "错误",
            ("zh", "alert_error_body") => "目前无法创建行程，请稍后再试。",
            ("zh", "alert_generic_error") => "发生错误。",
            ("zh", "no_ai_response") => "AI没有回复。",

            (_, "page_title") => "Tour Plan",
            (_, "header_title") => "Tour Plan Groq",
            (_, "header_subtitle") => "Lên lịch trình ẩm thực thông minh trong chớp mắt",
            (_, "step1") => "Chọn quán",
            (_, "step2") => "Sở thích",
            (_, "step3") => "Nhận Tour",
            (_, "suggest_title") => "Gợi ý món ngon",
            (_, "empty_card") => "Đã hết quán để quẹt hôm nay",
            (_, "load_more") => "Tải thêm quán",
            (_, "skip") => "Bỏ qua",
            (_, "like") => "Thích",
            (_, "personalize_title") => "Cá nhân hóa lịch trình",
            (_, "personalize_hint") => "Thêm ghi chú về thời gian, ngân sách hoặc yêu cầu đặc biệt:",
            (_, "preferences_placeholder") => "Ví dụ: Đi buổi tối, ngân sách 500k, thích không gian yên tĩnh...",
            (_, "generate_button") => "Tạo Tour AI ngay",
            (_, "loading") => "AI đang 'may đo' lịch trình dành riêng cho bạn...",
            (_, "access_denied_title") => "Nâng tầm trải nghiệm",
            (_, "access_denied_description") => "Mở khóa tính năng lập lịch trình AI không giới hạn với gói Tour Plus.",
            (_, "upgrade_button") => "Khám phá ngay",
            (_, "alert_missing_data_title") => "Thiếu dữ liệu",
            (_, "alert_missing_data_body") => "Bạn cần quẹt 'Thích' ít nhất 1 quán ăn trong mục Tinder để AI có thể lập lịch trình.",
            (_, "alert_error_title") => "Lỗi",
            (_, "alert_error_body") => "Không thể tạo lịch trình lúc này. Vui lòng thử lại sau.",
            (_, "alert_generic_error") => "Đã có lỗi xảy ra.",
            (_, "no_ai_response") => "Không có phản hồi từ AI.",
            _ => key
        };
    }
}
