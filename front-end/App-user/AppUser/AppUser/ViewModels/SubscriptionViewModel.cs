using AppUser.Models;
using AppUser.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Devices;

namespace AppUser.ViewModels;

public partial class SubscriptionViewModel : ObservableObject
{
    private static readonly TimeSpan PaymentPollingInterval = TimeSpan.FromSeconds(4);

    private readonly SubscriptionService _subscriptionService;
    private readonly AudioService _audioService;
    private readonly AuthService _authService;

    private CancellationTokenSource? _paymentPollingCts;
    private bool _paymentSuccessShown;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private List<AppServicePackageDto> packages = new();

    [ObservableProperty]
    private AppSubscriptionEnvelopeDto current = new();

    [ObservableProperty]
    private string title = "Goi dich vu";

    [ObservableProperty]
    private string pageHeading = "Goi kham pha am thuc";

    [ObservableProperty]
    private string currentPackageHeading = "Goi hien tai";

    [ObservableProperty]
    private string noActivePackageText = "Ban chua co goi Tour nao dang hoat dong.";

    [ObservableProperty]
    private string reloadText = "Kiem tra lai";

    [ObservableProperty]
    private string continuePaymentText = "Xem ma QR thanh toan";

    [ObservableProperty]
    private string cancelPackageText = "Huy goi";

    [ObservableProperty]
    private string choosePackageHeading = "Chon goi phu hop";

    [ObservableProperty]
    private string emptyPackagesText = "Hien tai chua co goi nao kha dung.";

    [ObservableProperty]
    private string qrDialogTitle = "Thanh toan goi Tour";

    [ObservableProperty]
    private string qrDialogHint = "Mo ung dung ngan hang hoac vi dien tu tren thiet bi khac de quet ma QR nay va hoan tat thanh toan.";

    [ObservableProperty]
    private string closeQrText = "Dong";

    [ObservableProperty]
    private string expiresOnFormat = "Het han: {0:dd/MM/yyyy}";

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

    [ObservableProperty]
    private double qrDialogWidth = 320;

    [ObservableProperty]
    private double qrDialogMaxHeight = 560;

    [ObservableProperty]
    private double qrImageSize = 220;

    [ObservableProperty]
    private bool showQrImage;

    [ObservableProperty]
    private bool isPaymentChecking;

    [ObservableProperty]
    private string paymentCheckingMessage = string.Empty;

    public string CurrentStatusDisplay => Current.Subscription == null
        ? string.Empty
        : LocalizeStatus(Current.Subscription.Status);

    public string CurrentExpiryDisplay => Current.Subscription == null
        ? string.Empty
        : string.Format(ExpiresOnFormat, Current.Subscription.EndDate);

    public SubscriptionViewModel(SubscriptionService subscriptionService, AudioService audioService, AuthService authService)
    {
        _subscriptionService = subscriptionService;
        _audioService = audioService;
        _authService = authService;
        _audioService.LanguageChanged += OnLanguageChanged;
        PaymentCheckingMessage = GetText("payment_checking_message");
        UpdateLocalizedTexts();
    }

    public async Task InitializeAsync()
    {
        UpdateDialogLayout(
            DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density,
            DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density);

        await ReloadAsync();
        await RestorePendingCheckoutAsync();
    }

    public void UpdateDialogLayout(double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        var safeWidth = Math.Max(240, width - 24);
        QrDialogWidth = Math.Min(420, safeWidth);
        QrDialogMaxHeight = Math.Max(320, height - 40);

        var candidateQrSize = Math.Min(QrDialogWidth - 32, QrDialogMaxHeight * 0.42);
        QrImageSize = Math.Max(180, Math.Min(320, candidateQrSize));
    }

    public void OnPageDisappearing()
    {
        StopPaymentPolling();
    }

    public async Task HandleAppResumedAsync()
    {
        if (Current.Subscription == null)
        {
            return;
        }

        if (IsPendingPaymentState(Current.Subscription.Status, Current.Subscription.PaymentStatus))
        {
            await SyncPaymentSilentlyAsync(Current.Subscription.Id, CancellationToken.None, showSuccessAlert: true);

            if (Current.Subscription != null && IsPendingPaymentState(Current.Subscription.Status, Current.Subscription.PaymentStatus))
            {
                StartPaymentPolling(Current.Subscription.Id);
            }
        }
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
            await Shell.Current.DisplayAlert(GetText("error_title"), ex.Message, GetText("ok"));
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SubscribeMonthlyAsync(AppServicePackageDto package)
    {
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
        if (PendingCheckout == null && Current.Subscription != null && IsPendingPaymentState(Current.Subscription.Status, Current.Subscription.PaymentStatus))
        {
            PendingPackageTier = Current.Subscription.PackageTier;
            PendingBillingCycle = Current.Subscription.BillingCycle;
            PendingCheckoutTitle = $"{Current.Subscription.PackageName} - {ResolveBillingCycleLabel(Current.Subscription.BillingCycle)}";
            PendingCheckout = new AppCheckoutSubscriptionResultDto
            {
                SubscriptionId = Current.Subscription.Id,
                CheckoutUrl = Current.Subscription.CheckoutUrl ?? string.Empty,
                PaymentLinkId = Current.Subscription.PaymentLinkId ?? string.Empty,
                Message = "Pending"
            };
            QrImageUrl = BuildQrImageUrl(null, Current.Subscription.CheckoutUrl);
            ShowQrImage = !string.IsNullOrWhiteSpace(QrImageUrl);
        }

        if (PendingCheckout != null)
        {
            IsQrVisible = true;
            StartPaymentPolling(PendingCheckout.SubscriptionId);
        }
    }

    [RelayCommand]
    private async Task SyncPaymentAsync()
    {
        if (Current.Subscription == null)
        {
            return;
        }

        await SyncPaymentSilentlyAsync(Current.Subscription.Id, CancellationToken.None, showSuccessAlert: true);
    }

    [RelayCommand]
    private async Task CancelCurrentAsync()
    {
        if (Current.Subscription == null) return;

        var ok = await _subscriptionService.CancelAsync(Current.Subscription.Id);
        if (ok)
        {
            StopPaymentPolling();
            ClearPendingCheckout();
            Current = new AppSubscriptionEnvelopeDto();
            await ReloadAsync();
            return;
        }

        await Shell.Current.DisplayAlert(GetText("error_title"), GetText("cancel_failed_message"), GetText("ok"));
    }

    [RelayCommand]
    private void CloseQr()
    {
        IsQrVisible = false;
        StopPaymentPolling();
    }

    private async Task SubscribeAsync(AppServicePackageDto package, string billingCycle)
    {
        if (package == null) return;

        await _authService.EnsureSessionLoadedAsync();
        await _authService.InitGuestSessionAsync();

        try
        {
            StatusMessage = null;

            var cycleToUse = string.IsNullOrWhiteSpace(package.RecommendedBillingCycle)
                ? billingCycle
                : package.RecommendedBillingCycle;

            var result = await _subscriptionService.CreateCheckoutAsync(package.Id, cycleToUse);
            if (result == null || string.IsNullOrWhiteSpace(result.CheckoutUrl))
            {
                await Shell.Current.DisplayAlert(GetText("error_title"), GetText("payment_link_failed_message"), GetText("ok"));
                return;
            }

            PendingPackageTier = package.Tier;
            PendingBillingCycle = cycleToUse;
            PendingCheckoutTitle = $"{LocalizePackageName(package.Tier, package.Name)} - {ResolveBillingCycleLabel(cycleToUse)}";
            PendingCheckout = result;
            QrImageUrl = BuildQrImageUrl(result.QrCode, result.CheckoutUrl);
            ShowQrImage = !string.IsNullOrWhiteSpace(QrImageUrl);
            IsQrVisible = true;
            PaymentCheckingMessage = GetText("payment_checking_message");
            _paymentSuccessShown = false;

            Current = LocalizeEnvelope(await _subscriptionService.GetMySubscriptionAsync());
            NormalizeCurrentSubscription();

            if (Current.Subscription == null)
            {
                Current = new AppSubscriptionEnvelopeDto
                {
                    HasSubscription = true,
                    Subscription = new AppCurrentSubscriptionDto
                    {
                        Id = result.SubscriptionId,
                        PackageName = LocalizePackageName(package.Tier, package.Name),
                        PackageTier = package.Tier,
                        BillingCycle = cycleToUse,
                        CheckoutUrl = result.CheckoutUrl,
                        PaymentLinkId = result.PaymentLinkId,
                        Status = "PendingPayment",
                        PaymentStatus = "Pending"
                    }
                };
            }

            StartPaymentPolling(result.SubscriptionId);

        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            await Shell.Current.DisplayAlert(GetText("error_title"), ex.Message, GetText("ok"));
        }
    }

    private async Task RestorePendingCheckoutAsync()
    {
        if (Current.Subscription == null)
        {
            ClearPendingCheckout();
            return;
        }

        if (!IsPendingPaymentState(Current.Subscription.Status, Current.Subscription.PaymentStatus) ||
            string.IsNullOrWhiteSpace(Current.Subscription.CheckoutUrl))
        {
            ClearPendingCheckout();
            return;
        }

        PendingPackageTier = Current.Subscription.PackageTier;
        PendingBillingCycle = Current.Subscription.BillingCycle;
        PendingCheckoutTitle = $"{Current.Subscription.PackageName} - {ResolveBillingCycleLabel(Current.Subscription.BillingCycle)}";
        PendingCheckout = new AppCheckoutSubscriptionResultDto
        {
            SubscriptionId = Current.Subscription.Id,
            CheckoutUrl = Current.Subscription.CheckoutUrl ?? string.Empty,
            PaymentLinkId = Current.Subscription.PaymentLinkId ?? string.Empty,
            Message = "Pending"
        };

        QrImageUrl = BuildQrImageUrl(null, Current.Subscription.CheckoutUrl);
        ShowQrImage = !string.IsNullOrWhiteSpace(QrImageUrl);
        IsQrVisible = true;
        PaymentCheckingMessage = GetText("payment_checking_message");
        StartPaymentPolling(Current.Subscription.Id);
        await Task.CompletedTask;
    }

    private void StartPaymentPolling(int subscriptionId)
    {
        StopPaymentPolling();

        _paymentPollingCts = new CancellationTokenSource();
        var token = _paymentPollingCts.Token;

        _ = RunPaymentPollingLoopAsync(subscriptionId, token);
    }

    private void StopPaymentPolling()
    {
        if (_paymentPollingCts == null)
        {
            IsPaymentChecking = false;
            return;
        }

        if (!_paymentPollingCts.IsCancellationRequested)
        {
            _paymentPollingCts.Cancel();
        }

        _paymentPollingCts.Dispose();
        _paymentPollingCts = null;
        IsPaymentChecking = false;
    }

    private async Task RunPaymentPollingLoopAsync(int subscriptionId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var shouldContinue = await SyncPaymentSilentlyAsync(subscriptionId, cancellationToken, showSuccessAlert: true);
            if (!shouldContinue)
            {
                break;
            }

            try
            {
                await Task.Delay(PaymentPollingInterval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<bool> SyncPaymentSilentlyAsync(int subscriptionId, CancellationToken cancellationToken, bool showSuccessAlert)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        try
        {
            IsPaymentChecking = true;
            PaymentCheckingMessage = GetText("payment_checking_message");

            var envelope = await _subscriptionService.TrySyncPaymentAsync(subscriptionId);
            if (envelope == null)
            {
                PaymentCheckingMessage = GetText("payment_checking_retry_message");
                return true;
            }

            Current = LocalizeEnvelope(envelope);
            NormalizeCurrentSubscription();

            if (Current.Subscription == null)
            {
                ClearPendingCheckout();
                IsQrVisible = false;
                StopPaymentPolling();
                return false;
            }

            PendingCheckout ??= new AppCheckoutSubscriptionResultDto
            {
                SubscriptionId = Current.Subscription.Id,
                CheckoutUrl = Current.Subscription.CheckoutUrl ?? string.Empty,
                PaymentLinkId = Current.Subscription.PaymentLinkId ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(QrImageUrl))
            {
                QrImageUrl = BuildQrImageUrl(null, Current.Subscription.CheckoutUrl);
                ShowQrImage = !string.IsNullOrWhiteSpace(QrImageUrl);
            }

            if (string.Equals(Current.Subscription.Status, "Active", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Current.Subscription.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            {
                StopPaymentPolling();
                ClearPendingCheckout();
                IsQrVisible = false;

                if (showSuccessAlert && !_paymentSuccessShown)
                {
                    _paymentSuccessShown = true;
                    await Shell.Current.DisplayAlert(GetText("payment_success_title"), GetText("payment_success_message"), GetText("ok"));
                }

                return false;
            }

            if (string.Equals(Current.Subscription.PaymentStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                StopPaymentPolling();
                PaymentCheckingMessage = GetText("payment_cancelled_message");
                await Shell.Current.DisplayAlert(GetText("payment_cancelled_title"), GetText("payment_cancelled_message"), GetText("ok"));
                return false;
            }

            if (string.Equals(Current.Subscription.PaymentStatus, "Failed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Current.Subscription.Status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Current.Subscription.Status, "Expired", StringComparison.OrdinalIgnoreCase))
            {
                StopPaymentPolling();
                PaymentCheckingMessage = GetText("payment_failed_message");
                await Shell.Current.DisplayAlert(GetText("payment_failed_title"), GetText("payment_failed_message"), GetText("ok"));
                return false;
            }

            PaymentCheckingMessage = GetText("payment_checking_message");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Subscription polling error: {ex}");
            PaymentCheckingMessage = GetText("payment_checking_retry_message");
            return true;
        }
        finally
        {
            IsPaymentChecking = _paymentPollingCts != null && !_paymentPollingCts.IsCancellationRequested;
        }
    }

    private void ClearPendingCheckout()
    {
        PendingCheckout = null;
        PendingCheckoutTitle = string.Empty;
        PendingPackageTier = string.Empty;
        PendingBillingCycle = string.Empty;
        QrImageUrl = string.Empty;
        ShowQrImage = false;
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

            return $"https://api.qrserver.com/v1/create-qr-code/?size=640x640&data={Uri.EscapeDataString(qrCode)}";
        }

        if (string.IsNullOrWhiteSpace(checkoutUrl))
        {
            return string.Empty;
        }

        return $"https://api.qrserver.com/v1/create-qr-code/?size=640x640&data={Uri.EscapeDataString(checkoutUrl)}";
    }

    private static bool IsPendingPaymentState(string status, string paymentStatus)
    {
        return string.Equals(status, "PendingPayment", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(status, "Pending", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(paymentStatus, "Pending", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveBillingCycleLabel(string? billingCycle) => billingCycle switch
    {
        "Daily" => "Theo ngay",
        "Yearly" => "Theo nam",
        _ => "Theo thang"
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

        if (!string.IsNullOrWhiteSpace(PendingPackageTier))
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
        CancelPackageText = GetText("cancel_package");
        ChoosePackageHeading = GetText("choose_package_heading");
        EmptyPackagesText = GetText("empty_packages");
        QrDialogTitle = GetText("qr_dialog_title");
        QrDialogHint = GetText("qr_dialog_hint");
        CloseQrText = GetText("close_qr");
        ExpiresOnFormat = GetText("expires_on_format");
        PaymentCheckingMessage = GetText("payment_checking_message");
    }

    private List<AppServicePackageDto> LocalizePackages(IEnumerable<AppServicePackageDto> sourcePackages)
    {
        var hasBasic = Current.Subscription?.PackageTier == "TourBasic" &&
                       string.Equals(Current.Subscription.Status, "Active", StringComparison.OrdinalIgnoreCase);

        return sourcePackages
            .GroupBy(p => p.Tier, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.OrderBy(x => x.Id).First())
            .Select(package =>
            {
                var price = package.MonthlyPrice;
                var displayPrice = package.DisplayPrice;

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
            })
            .ToList();
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
        ("zh", "TourBasic") => "Jichu Tour",
        ("zh", "TourPlus") => "Gaoji Tour",
        ("vi", "TourBasic") => "Tour Basic",
        ("vi", "TourPlus") => "Tour Plus",
        _ => fallback
    };

    private string LocalizePackageDescription(string tier) => (_audioService.CurrentLanguage, tier) switch
    {
        ("en", "TourBasic") => "Unlock audio narration when near food stalls. Valid for 1 day.",
        ("en", "TourPlus") => "Full experience: narration + Food Tinder + AI itinerary + Chatbot. Valid for 1 day.",
        ("zh", "TourBasic") => "Khi den gan quan an, app mo khoa thuyet minh audio trong 1 ngay.",
        ("zh", "TourPlus") => "Trai nghiem day du: audio + Food Tinder + AI itinerary + Chatbot trong 1 ngay.",
        ("vi", "TourBasic") => "Mo khoa thuyet minh am thuc khi den gan quan. Su dung trong 1 ngay.",
        ("vi", "TourPlus") => "Trai nghiem day du: thuyet minh + Tinder am thuc + AI lich trinh + Chatbot. Su dung trong 1 ngay.",
        _ => string.Empty
    };

    private List<string> LocalizePackageFeatures(string tier) => (_audioService.CurrentLanguage, tier) switch
    {
        ("en", "TourBasic") => new() { "Valid for 1 day", "Auto-play narration near POI", "Listen in 3 languages", "Review after listening", "Tho Dia chatbot support" },
        ("en", "TourPlus") => new() { "Valid for 1 day", "All Tour Basic features", "Food Tinder", "AI tour planner", "Priority store suggestions" },
        ("zh", "TourBasic") => new() { "Hieu luc 1 ngay", "Tu dong phat audio khi den gan", "Nghe 3 ngon ngu", "Co the review sau khi nghe", "Chatbot tho dia" },
        ("zh", "TourPlus") => new() { "Hieu luc 1 ngay", "Tat ca tinh nang Tour Basic", "Food Tinder", "AI tour planner", "Uu tien de xuat quan hot" },
        ("vi", "TourBasic") => new() { "Su dung trong 1 ngay", "Tu dong phat thuyet minh khi den gan POI", "Nghe 3 ngon ngu", "Ho tro review sau khi nghe", "Chatbot Tho Dia tu van mon an" },
        ("vi", "TourPlus") => new() { "Su dung trong 1 ngay", "Tat ca quyen loi Tour Basic", "Tinder am thuc", "AI ke hoach Tour", "Uu tien de xuat quan hot" },
        _ => new()
    };

    private string LocalizeStatus(string status) => (_audioService.CurrentLanguage, status) switch
    {
        ("en", "Active") => "Active",
        ("en", "PendingPayment") => "Pending payment",
        ("en", "Pending") => "Pending",
        ("en", "Cancelled") => "Cancelled",
        ("en", "Expired") => "Expired",
        ("zh", "Active") => "Active",
        ("zh", "PendingPayment") => "Pending payment",
        ("zh", "Pending") => "Pending",
        ("zh", "Cancelled") => "Cancelled",
        ("zh", "Expired") => "Expired",
        ("vi", "Active") => "Dang hoat dong",
        ("vi", "PendingPayment") => "Cho thanh toan",
        ("vi", "Pending") => "Dang xu ly",
        ("vi", "Cancelled") => "Da huy",
        ("vi", "Expired") => "Het han",
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
            ("en", "cancel_package") => "Cancel package",
            ("en", "choose_package_heading") => "Choose the right package",
            ("en", "empty_packages") => "There are currently no available packages.",
            ("en", "qr_dialog_title") => "Tour package payment",
            ("en", "qr_dialog_hint") => "If you are paying on this phone, open PayOS directly. Keep the QR code for another device if needed.",
            ("en", "open_payos") => "Open PayOS",
            ("en", "close_qr") => "Close",
            ("en", "expires_on_format") => "Expires on: {0:dd/MM/yyyy}",
            ("en", "payment_success_title") => "Payment successful",
            ("en", "payment_success_message") => "Your tour package has been activated.",
            ("en", "payment_cancelled_title") => "Payment cancelled",
            ("en", "payment_cancelled_message") => "The tour package payment was cancelled.",
            ("en", "payment_failed_title") => "Payment failed",
            ("en", "payment_failed_message") => "PayOS has not confirmed a successful payment.",
            ("en", "payment_ready_title") => "Ready to pay",
            ("en", "payment_link_failed_message") => "Unable to create the PayOS payment link.",
            ("en", "payment_checking_message") => "Checking payment status automatically...",
            ("en", "payment_checking_retry_message") => "Network is unstable. We will check again automatically.",
            ("en", "error_title") => "Error",
            ("en", "cancel_failed_message") => "Unable to cancel the current package.",
            ("en", "ok") => "OK",

            ("zh", "page_title") => "Tour taocan",
            ("zh", "page_heading") => "Meishi tour taocan",
            ("zh", "current_package_heading") => "Dangqian taocan",
            ("zh", "no_active_package") => "Ni hai meiyou qiyong zhong de tour taocan.",
            ("zh", "reload") => "Shuaxin",
            ("zh", "continue_payment") => "Jixu zhifu",
            ("zh", "cancel_package") => "Quxiao taocan",
            ("zh", "choose_package_heading") => "Xuanze heshi de taocan",
            ("zh", "empty_packages") => "Dangqian meiyou keyong taocan.",
            ("zh", "qr_dialog_title") => "Tour zhifu",
            ("zh", "qr_dialog_hint") => "Ruguo ni zai ben ji zhifu, qing zhijie daka PayOS. QR ma yong yu qita shebei sao ma.",
            ("zh", "open_payos") => "Dakai PayOS",
            ("zh", "close_qr") => "Guanbi",
            ("zh", "expires_on_format") => "Daoqi ri: {0:dd/MM/yyyy}",
            ("zh", "payment_success_title") => "Zhifu chenggong",
            ("zh", "payment_success_message") => "Ni de tour taocan yi qiyong.",
            ("zh", "payment_cancelled_title") => "Zhifu yi quxiao",
            ("zh", "payment_cancelled_message") => "Taocan zhifu da yi quxiao.",
            ("zh", "payment_failed_title") => "Zhifu shibai",
            ("zh", "payment_failed_message") => "PayOS hai meiyou queren zhifu chenggong.",
            ("zh", "payment_ready_title") => "Keyi zhifu",
            ("zh", "payment_link_failed_message") => "Wufa chuangjian PayOS zhifu lianjie.",
            ("zh", "payment_checking_message") => "Zidong jiancha zhifu zhuangtai...",
            ("zh", "payment_checking_retry_message") => "Wangluo bu wending. Xitong hui tuong bo zai.",
            ("zh", "error_title") => "Cuowu",
            ("zh", "cancel_failed_message") => "Wufa quxiao dangqian taocan.",
            ("zh", "ok") => "OK",

            (_, "page_title") => "Goi dich vu",
            (_, "page_heading") => "Goi kham pha am thuc",
            (_, "current_package_heading") => "Goi hien tai",
            (_, "no_active_package") => "Ban chua co goi Tour nao dang hoat dong.",
            (_, "reload") => "Kiem tra lai",
            (_, "continue_payment") => "Tiep tuc thanh toan",
            (_, "cancel_package") => "Huy goi",
            (_, "choose_package_heading") => "Chon goi phu hop",
            (_, "empty_packages") => "Hien tai chua co goi nao kha dung.",
            (_, "qr_dialog_title") => "Thanh toan goi Tour",
            (_, "qr_dialog_hint") => "Neu thanh toan tren dien thoai nay, hay bam mo PayOS. Ma QR duoc giu lai de quet tu thiet bi khac neu can.",
            (_, "open_payos") => "Mo cong PayOS",
            (_, "close_qr") => "Dong",
            (_, "expires_on_format") => "Het han: {0:dd/MM/yyyy}",
            (_, "payment_success_title") => "Thanh toan thanh cong",
            (_, "payment_success_message") => "Goi Tour cua ban da duoc kich hoat.",
            (_, "payment_cancelled_title") => "Da huy thanh toan",
            (_, "payment_cancelled_message") => "Giao dich thanh toan da bi huy.",
            (_, "payment_failed_title") => "Thanh toan that bai",
            (_, "payment_failed_message") => "PayOS chua xac nhan thanh toan thanh cong.",
            (_, "payment_ready_title") => "San sang thanh toan",
            (_, "payment_link_failed_message") => "Khong tao duoc lien ket thanh toan PayOS.",
            (_, "payment_checking_message") => "Dang tu dong kiem tra trang thai thanh toan...",
            (_, "payment_checking_retry_message") => "Ket noi dang chua on dinh. He thong se tu kiem tra lai.",
            (_, "error_title") => "Loi",
            (_, "cancel_failed_message") => "Khong huy duoc goi hien tai.",
            (_, "ok") => "OK",
            _ => key
        };
    }
}
