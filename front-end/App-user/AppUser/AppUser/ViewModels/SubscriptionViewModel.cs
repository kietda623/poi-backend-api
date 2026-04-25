using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using Microsoft.Maui.ApplicationModel;

namespace AppUser.ViewModels;

public partial class SubscriptionViewModel : ObservableObject
{
    private readonly SubscriptionService _subscriptionService;
    private readonly AudioService _audioService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private List<AppServicePackageDto> packages = new();

    [ObservableProperty]
    private AppSubscriptionEnvelopeDto current = new();

    [ObservableProperty]
    private string title = "Gói dịch vụ";

    [ObservableProperty]
    private string pageHeading = "Gói khám phá ẩm thực";

    [ObservableProperty]
    private string currentPackageHeading = "Gói hiện tại";

    [ObservableProperty]
    private string noActivePackageText = "Bạn chưa có gói Tour nào đang hoạt động.";

    [ObservableProperty]
    private string reloadText = "Kiểm tra lại";

    [ObservableProperty]
    private string continuePaymentText = "Tiếp tục thanh toán";

    [ObservableProperty]
    private string paymentCompletedText = "Tôi đã thanh toán";

    [ObservableProperty]
    private string cancelPackageText = "Hủy gói";

    [ObservableProperty]
    private string choosePackageHeading = "Chọn gói phù hợp";

    [ObservableProperty]
    private string emptyPackagesText = "Hiện tại chưa có gói nào khả dụng.";

    [ObservableProperty]
    private string qrDialogTitle = "Thanh toán gói Tour";

    [ObservableProperty]
    private string qrDialogHint = "Quét mã QR để thanh toán. Sau khi thanh toán xong, bấm 'Tôi đã thanh toán' để cập nhật trạng thái gói.";

    [ObservableProperty]
    private string openPayOsText = "Mở cổng PayOS";

    [ObservableProperty]
    private string closeQrText = "Đóng QR";

    [ObservableProperty]
    private string expiresOnFormat = "Hết hạn: {0:dd/MM/yyyy}";

    public string CurrentStatusDisplay => Current.Subscription == null
        ? string.Empty
        : LocalizeStatus(Current.Subscription.Status);

    public string CurrentExpiryDisplay => Current.Subscription == null
        ? string.Empty
        : string.Format(ExpiresOnFormat, Current.Subscription.EndDate);

    [ObservableProperty]
    private string? statusMessage;

    [ObservableProperty]
    private AppCheckoutSubscriptionResultDto? pendingCheckout;

    [ObservableProperty]
    private bool isQrVisible;

    [ObservableProperty]
    private string qrImageUrl = string.Empty;

    [ObservableProperty]
    private string pendingCheckoutTitle = string.Empty;

    [ObservableProperty]
    private string pendingPackageTier = string.Empty;

    [ObservableProperty]
    private string pendingBillingCycle = string.Empty;

    public SubscriptionViewModel(SubscriptionService subscriptionService, AudioService audioService, AuthService authService)
    {
        _subscriptionService = subscriptionService;
        _audioService = audioService;
        _authService = authService;
        _audioService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedTexts();
    }

    public async Task InitializeAsync()
    {
        await ReloadAsync();
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = null;
            var fetchedPackages = await _subscriptionService.GetUserPackagesAsync();
            var fetchedCurrent = await _subscriptionService.GetMySubscriptionAsync();
            Packages = LocalizePackages(fetchedPackages);
            Current = LocalizeEnvelope(fetchedCurrent);
            NormalizeCurrentSubscription();
        }
        catch (Exception ex)
        {
            Packages = new();
            Current = new();
            StatusMessage = ex.Message;
            await Shell.Current.DisplayAlert("Loi", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SubscribeMonthlyAsync(AppServicePackageDto package)
    {
        // Tour packages use Daily billing
        await SubscribeAsync(package, "Daily");
    }

    [RelayCommand]
    private async Task SubscribeYearlyAsync(AppServicePackageDto package)
    {
        await SubscribeAsync(package, "Yearly");
    }

    [RelayCommand]
    private async Task ContinuePaymentAsync()
    {
        if (!string.IsNullOrWhiteSpace(PendingCheckout?.CheckoutUrl))
        {
            await Browser.Default.OpenAsync(PendingCheckout.CheckoutUrl, BrowserLaunchMode.SystemPreferred);
            return;
        }

        if (!string.IsNullOrWhiteSpace(Current.Subscription?.CheckoutUrl))
        {
            await Browser.Default.OpenAsync(Current.Subscription.CheckoutUrl, BrowserLaunchMode.SystemPreferred);
        }
    }

    [RelayCommand]
    private async Task SyncPaymentAsync()
    {
        if (Current.Subscription == null) return;

        try
        {
            Current = LocalizeEnvelope(await _subscriptionService.SyncPaymentAsync(Current.Subscription.Id));
            NormalizeCurrentSubscription();

            if (Current.Subscription?.Status == "Active" && Current.Subscription.PaymentStatus == "Paid")
            {
                IsQrVisible = false;
                await Shell.Current.DisplayAlert(GetText("payment_success_title"), GetText("payment_success_message"), GetText("ok"));
                return;
            }

            if (Current.Subscription?.PaymentStatus == "Cancelled")
            {
                IsQrVisible = false;
                await Shell.Current.DisplayAlert(GetText("payment_cancelled_title"), GetText("payment_cancelled_message"), GetText("ok"));
                return;
            }

            if (Current.Subscription?.PaymentStatus == "Failed")
            {
                IsQrVisible = false;
                await Shell.Current.DisplayAlert(GetText("payment_failed_title"), GetText("payment_failed_message"), GetText("ok"));
                return;
            }

            await Shell.Current.DisplayAlert(GetText("processing_title"), GetText("processing_message"), GetText("ok"));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert(GetText("error_title"), ex.Message, GetText("ok"));
        }
    }

    [RelayCommand]
    private async Task CancelCurrentAsync()
    {
        if (Current.Subscription == null) return;
        var ok = await _subscriptionService.CancelAsync(Current.Subscription.Id);
        if (ok)
        {
            PendingCheckout = null;
            IsQrVisible = false;
            QrImageUrl = string.Empty;
            Current = new AppSubscriptionEnvelopeDto();
            await ReloadAsync();
            return;
        }

        await Shell.Current.DisplayAlert(GetText("error_title"), GetText("cancel_failed_message"), GetText("ok"));
    }

    private async Task SubscribeAsync(AppServicePackageDto package, string billingCycle)
    {
        if (package == null) return;

        await _authService.EnsureSessionLoadedAsync();
        if (!_authService.IsLoggedIn)
        {
            var goToLogin = await Shell.Current.DisplayAlert(
                "Dang nhap de dang ky goi",
                "Ban can dang nhap truoc khi tao thanh toan cho goi audio.",
                "Dang nhap",
                "De sau");

            if (goToLogin)
            {
                await Shell.Current.GoToAsync("login");
            }

            return;
        }

        try
        {
            StatusMessage = null;
            var cycleToUse = string.IsNullOrWhiteSpace(package.RecommendedBillingCycle) ? billingCycle : package.RecommendedBillingCycle;
            var result = await _subscriptionService.CreateCheckoutAsync(package.Id, cycleToUse);
            if (result == null || string.IsNullOrWhiteSpace(result.CheckoutUrl))
            {
                await Shell.Current.DisplayAlert("Loi", "Khong tao duoc link thanh toan PayOS.", "OK");
                return;
            }

            PendingCheckout = result;
            PendingPackageTier = package.Tier;
            PendingBillingCycle = cycleToUse;
            PendingCheckoutTitle = $"{LocalizePackageName(package.Tier, package.Name)} - {ResolveBillingCycleLabel(cycleToUse)}";
            QrImageUrl = BuildQrImageUrl(result.QrCode, result.CheckoutUrl);
            IsQrVisible = !string.IsNullOrWhiteSpace(QrImageUrl);
            Current = LocalizeEnvelope(await _subscriptionService.GetMySubscriptionAsync());
            NormalizeCurrentSubscription();
            var paymentReadyMessage = IsQrVisible
                ? GetText("payment_ready_qr_message")
                : GetText("payment_ready_link_message");
            await Shell.Current.DisplayAlert(GetText("payment_ready_title"), paymentReadyMessage, GetText("ok"));
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            await Shell.Current.DisplayAlert(GetText("error_title"), ex.Message, GetText("ok"));
        }
    }

    [RelayCommand]
    private void CloseQr()
    {
        IsQrVisible = false;
    }

    private static string BuildQrImageUrl(string? qrCode, string? checkoutUrl)
    {
        if (!string.IsNullOrWhiteSpace(qrCode))
        {
            if (Uri.TryCreate(qrCode, UriKind.Absolute, out var qrUri))
            {
                return qrUri.ToString();
            }

            if (qrCode.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                return qrCode;
            }

            return $"https://api.qrserver.com/v1/create-qr-code/?size=280x280&data={Uri.EscapeDataString(qrCode)}";
        }

        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return string.Empty;
        }

        return $"https://api.qrserver.com/v1/create-qr-code/?size=280x280&data={Uri.EscapeDataString(checkoutUrl)}";
    }

    private static string ResolveBillingCycleLabel(string? billingCycle) => billingCycle switch
    {
        "Daily" => "Theo ngày",
        "Yearly" => "Theo năm",
        _ => "Theo tháng"
    };

    private void NormalizeCurrentSubscription()
    {
        if (Current.Subscription == null)
        {
            return;
        }

        if (string.Equals(Current.Subscription.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(Current.Subscription.Status, "Expired", StringComparison.OrdinalIgnoreCase))
        {
            Current = new AppSubscriptionEnvelopeDto();
        }
    }

    partial void OnCurrentChanged(AppSubscriptionEnvelopeDto value)
    {
        OnPropertyChanged(nameof(CurrentStatusDisplay));
        OnPropertyChanged(nameof(CurrentExpiryDisplay));
    }

    private void OnLanguageChanged(object? sender, string language)
    {
        UpdateLocalizedTexts();
        Packages = LocalizePackages(Packages);
        Current = LocalizeEnvelope(Current);
        if (PendingCheckout != null)
        {
            PendingCheckoutTitle = $"{LocalizePackageName(PendingPackageTier, PendingPackageTier)} - {ResolveBillingCycleLabel(PendingBillingCycle)}";
        }
        OnPropertyChanged(nameof(CurrentStatusDisplay));
        OnPropertyChanged(nameof(CurrentExpiryDisplay));
    }

    private void UpdateLocalizedTexts()
    {
        Title = GetText("page_title");
        PageHeading = GetText("page_heading");
        CurrentPackageHeading = GetText("current_package_heading");
        NoActivePackageText = GetText("no_active_package");
        ReloadText = GetText("reload");
        ContinuePaymentText = GetText("continue_payment");
        PaymentCompletedText = GetText("payment_completed");
        CancelPackageText = GetText("cancel_package");
        ChoosePackageHeading = GetText("choose_package_heading");
        EmptyPackagesText = GetText("empty_packages");
        QrDialogTitle = GetText("qr_dialog_title");
        QrDialogHint = GetText("qr_dialog_hint");
        OpenPayOsText = GetText("open_payos");
        CloseQrText = GetText("close_qr");
        ExpiresOnFormat = GetText("expires_on_format");
    }

    private List<AppServicePackageDto> LocalizePackages(IEnumerable<AppServicePackageDto> packages)
    {
        var hasBasic = Current.Subscription?.PackageTier == "TourBasic" && Current.Subscription.Status == "Active";

        return packages
        .GroupBy(p => p.Tier, StringComparer.OrdinalIgnoreCase)
        .Select(g => g.OrderBy(x => x.Id).First())
        .Select(package => 
        {
            var price = package.MonthlyPrice;
            var displayPrice = package.DisplayPrice;

            // Upgrade logic: if user has TourBasic, TourPlus cost 50K
            if (hasBasic && package.Tier == "TourPlus")
            {
                price = 50000;
                displayPrice = 50000;
            }

            return new AppServicePackageDto
            {
                Id = package.Id,
                Tier = package.Tier,
                Audience = package.Audience,
                MonthlyPrice = price,
                YearlyPrice = package.YearlyPrice,
                AllowAudioAccess = package.AllowAudioAccess,
                RecommendedBillingCycle = package.RecommendedBillingCycle,
                DisplayPrice = displayPrice,
                Name = LocalizePackageName(package.Tier, package.Name),
                Description = LocalizePackageDescription(package.Tier),
                DisplayLabel = ResolveBillingCycleLabel(package.RecommendedBillingCycle),
                Features = LocalizePackageFeatures(package.Tier)
            };
        }).ToList();
    }

    private AppSubscriptionEnvelopeDto LocalizeEnvelope(AppSubscriptionEnvelopeDto envelope)
    {
        if (envelope.Subscription == null)
        {
            return envelope;
        }

        envelope.Subscription.PackageName = LocalizePackageName(envelope.Subscription.PackageTier, envelope.Subscription.PackageName);
        return envelope;
    }

    private string LocalizePackageName(string tier, string fallback) => (_audioService.CurrentLanguage, tier) switch
    {
        ("en", "TourBasic") => "Tour Basic",
        ("en", "TourPlus") => "Tour Plus",
        ("zh", "TourBasic") => "基础游览",
        ("zh", "TourPlus") => "高级游览",
        ("vi", "TourBasic") => "Tour Basic",
        ("vi", "TourPlus") => "Tour Plus",
        _ => fallback
    };

    private string LocalizePackageDescription(string tier) => (_audioService.CurrentLanguage, tier) switch
    {
        ("en", "TourBasic") => "Unlock audio narration when near food stalls. Valid for 1 day.",
        ("en", "TourPlus") => "Full experience: narration + Food Tinder + AI itinerary + Chatbot. Valid for 1 day.",
        ("zh", "TourBasic") => "靠近摊位时自动播放美食讲解。有效期1天。",
        ("zh", "TourPlus") => "完整体验：讲解 + 美食Tinder + AI行程 + 聊天机器人。有效期1天。",
        ("vi", "TourBasic") => "Mở khóa thuyết minh ẩm thực khi đến gần quán. Sử dụng trong 1 ngày.",
        ("vi", "TourPlus") => "Trải nghiệm đầy đủ: thuyết minh + Tinder ẩm thực + AI lịch trình + Chatbot. Sử dụng trong 1 ngày.",
        _ => string.Empty
    };

    private List<string> LocalizePackageFeatures(string tier) => (_audioService.CurrentLanguage, tier) switch
    {
        ("en", "TourBasic") => new() { "Valid for 1 day", "Auto-play narration near POI", "Listen in 3 languages", "Review after listening", "Tho Dia Chatbot assistant", "No Food Tinder", "No AI Tour Planner" },
        ("en", "TourPlus") => new() { "Valid for 1 day", "All Tour Basic features", "Food Tinder (swipe left/right)", "AI Tour Planner by Groq", "Tho Dia Chatbot assistant", "Priority store suggestions" },
        ("zh", "TourBasic") => new() { "有效期1天", "靠近时自动播放讲解", "支持3种语言", "听后可评价", "土地公聊天机器人", "无美食Tinder", "无AI行程规划" },
        ("zh", "TourPlus") => new() { "有效期1天", "包含基础游览全部功能", "美食Tinder（左右滑动）", "AI行程规划 (Groq)", "土地公聊天机器人", "优先推荐热门商家" },
        ("vi", "TourBasic") => new() { "Sử dụng trong 1 ngày", "Tự động phát thuyết minh khi đến gần POI", "Nghe thuyết minh 3 ngôn ngữ", "Hỗ trợ review sau khi nghe", "Chatbot Thổ Địa tư vấn món ăn", "Không hỗ trợ Tinder Ẩm Thực", "Không hỗ trợ AI Lịch Trình" },
        ("vi", "TourPlus") => new() { "Sử dụng trong 1 ngày", "Tất cả quyền lợi Tour Basic", "Tinder Ẩm Thực (quẹt trái/phải)", "AI Kế Hoạch Tour từ Groq", "Chatbot Thổ Địa tư vấn món ăn", "Ưu tiên đề xuất quán cực hot" },
        _ => new()
    };

    private string LocalizeStatus(string status) => (_audioService.CurrentLanguage, status) switch
    {
        ("en", "Active") => "Active",
        ("en", "PendingPayment") => "Pending payment",
        ("en", "Pending") => "Pending",
        ("en", "Cancelled") => "Cancelled",
        ("en", "Expired") => "Expired",
        ("zh", "Active") => "已启用",
        ("zh", "PendingPayment") => "待支付",
        ("zh", "Pending") => "处理中",
        ("zh", "Cancelled") => "已取消",
        ("zh", "Expired") => "已过期",
        ("vi", "Active") => "Đang hoạt động",
        ("vi", "PendingPayment") => "Chờ thanh toán",
        ("vi", "Pending") => "Đang xử lý",
        ("vi", "Cancelled") => "Đã hủy",
        ("vi", "Expired") => "Hết hạn",
        _ => status
    };

    private string GetText(string key)
    {
        var lang = _audioService.CurrentLanguage;
        return (lang, key) switch
        {
            ("en", "page_title") => "Tour packages",
            ("en", "page_heading") => "Food tour packages",
            ("en", "current_package_heading") => "Current package",
            ("en", "no_active_package") => "You do not have an active tour package.",
            ("en", "reload") => "Refresh",
            ("en", "continue_payment") => "Continue payment",
            ("en", "payment_completed") => "I've paid",
            ("en", "cancel_package") => "Cancel package",
            ("en", "choose_package_heading") => "Choose the right package",
            ("en", "empty_packages") => "There are currently no available packages.",
            ("en", "qr_dialog_title") => "Tour package payment",
            ("en", "qr_dialog_hint") => "Scan the QR code to pay. After payment, tap 'I've paid' to update your package status.",
            ("en", "open_payos") => "Open PayOS",
            ("en", "close_qr") => "Close QR",
            ("en", "expires_on_format") => "Expires on: {0:dd/MM/yyyy}",
            ("en", "payment_success_title") => "Payment successful",
            ("en", "payment_success_message") => "Your tour package has been activated.",
            ("en", "payment_cancelled_title") => "Payment cancelled",
            ("en", "payment_cancelled_message") => "The tour package was cancelled.",
            ("en", "payment_failed_title") => "Payment failed",
            ("en", "payment_failed_message") => "PayOS has not confirmed a successful payment yet.",
            ("en", "processing_title") => "Processing",
            ("en", "processing_message") => "The system has not received the payment result yet. Please try again in a few seconds.",
            ("en", "error_title") => "Error",
            ("en", "cancel_failed_message") => "Unable to cancel the current package.",
            ("en", "payment_ready_title") => "Ready to pay",
            ("en", "payment_ready_qr_message") => "A QR code has been created in the app. You can scan it or open PayOS to complete payment.",
            ("en", "payment_ready_link_message") => "A PayOS payment link has been created. You can open PayOS to complete payment.",
            ("en", "ok") => "OK",

            ("zh", "page_title") => "游览套餐",
            ("zh", "page_heading") => "美食游览套餐",
            ("zh", "current_package_heading") => "当前套餐",
            ("zh", "no_active_package") => "你当前没有启用中的游览套餐。",
            ("zh", "reload") => "刷新",
            ("zh", "continue_payment") => "继续支付",
            ("zh", "payment_completed") => "我已支付",
            ("zh", "cancel_package") => "取消套餐",
            ("zh", "choose_package_heading") => "选择适合你的套餐",
            ("zh", "empty_packages") => "当前没有可用套餐。",
            ("zh", "qr_dialog_title") => "游览套餐支付",
            ("zh", "qr_dialog_hint") => "请扫描二维码完成支付。支付完成后，点击“我已支付”以更新套餐状态。",
            ("zh", "open_payos") => "打开 PayOS",
            ("zh", "close_qr") => "关闭二维码",
            ("zh", "expires_on_format") => "到期日: {0:dd/MM/yyyy}",
            ("zh", "payment_success_title") => "支付成功",
            ("zh", "payment_success_message") => "你的游览套餐已启用。",
            ("zh", "payment_cancelled_title") => "支付已取消",
            ("zh", "payment_cancelled_message") => "游览套餐已被取消。",
            ("zh", "payment_failed_title") => "支付失败",
            ("zh", "payment_failed_message") => "PayOS 尚未确认支付成功。",
            ("zh", "processing_title") => "处理中",
            ("zh", "processing_message") => "系统暂未收到支付结果，请稍后再试。",
            ("zh", "error_title") => "错误",
            ("zh", "cancel_failed_message") => "无法取消当前套餐。",
            ("zh", "payment_ready_title") => "准备支付",
            ("zh", "payment_ready_qr_message") => "应用内已生成二维码。你可以扫码或打开 PayOS 完成支付。",
            ("zh", "payment_ready_link_message") => "已生成 PayOS 支付链接。你可以打开 PayOS 完成支付。",
            ("zh", "ok") => "确定",

            (_, "page_title") => "Gói dịch vụ",
            (_, "page_heading") => "Gói khám phá ẩm thực",
            (_, "current_package_heading") => "Gói hiện tại",
            (_, "no_active_package") => "Bạn chưa có gói Tour nào đang hoạt động.",
            (_, "reload") => "Kiểm tra lại",
            (_, "continue_payment") => "Tiếp tục thanh toán",
            (_, "payment_completed") => "Tôi đã thanh toán",
            (_, "cancel_package") => "Hủy gói",
            (_, "choose_package_heading") => "Chọn gói phù hợp",
            (_, "empty_packages") => "Hiện tại chưa có gói nào khả dụng.",
            (_, "qr_dialog_title") => "Thanh toán gói Tour",
            (_, "qr_dialog_hint") => "Quét mã QR để thanh toán. Sau khi thanh toán xong, bấm 'Tôi đã thanh toán' để cập nhật trạng thái gói.",
            (_, "open_payos") => "Mở cổng PayOS",
            (_, "close_qr") => "Đóng QR",
            (_, "expires_on_format") => "Hết hạn: {0:dd/MM/yyyy}",
            (_, "payment_success_title") => "Thanh toán thành công",
            (_, "payment_success_message") => "Gói Tour của bạn đã được kích hoạt.",
            (_, "payment_cancelled_title") => "Đã hủy thanh toán",
            (_, "payment_cancelled_message") => "Gói Tour đã bị hủy.",
            (_, "payment_failed_title") => "Thanh toán thất bại",
            (_, "payment_failed_message") => "PayOS chưa xác nhận thanh toán thành công.",
            (_, "processing_title") => "Đang xử lý",
            (_, "processing_message") => "Hệ thống chưa nhận được kết quả thanh toán. Vui lòng thử lại sau ít giây.",
            (_, "error_title") => "Lỗi",
            (_, "cancel_failed_message") => "Không hủy được gói hiện tại.",
            (_, "payment_ready_title") => "Sẵn sàng thanh toán",
            (_, "payment_ready_qr_message") => "Mã QR đã được tạo trong app. Bạn có thể quét QR hoặc mở cổng PayOS để thanh toán.",
            (_, "payment_ready_link_message") => "Liên kết thanh toán PayOS đã được tạo. Bạn có thể mở cổng PayOS để thanh toán.",
            (_, "ok") => "OK",
            _ => key
        };
    }

}
