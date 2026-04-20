using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels;

public partial class ChatMessage : ObservableObject
{
    public string Text { get; set; } = string.Empty;
    public bool IsFromUser { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public partial class ChatViewModel : ObservableObject
{
    private readonly AiService _aiService;
    private readonly AudioService _audioService;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isAccessDenied;

    [ObservableProperty]
    private string userMessage = string.Empty;

    // Localized UI strings
    [ObservableProperty]
    private string pageTitle = "Thổ Địa Vĩnh Khánh";

    [ObservableProperty]
    private string onlineStatus = "Đang trực tuyến";

    [ObservableProperty]
    private string inputPlaceholder = "Hỏi Thổ Địa cái gì hay ho...";

    [ObservableProperty]
    private string typingIndicator = "Thổ Địa đang gõ...";

    [ObservableProperty]
    private string accessDeniedTitle = "Trò chuyện với Thổ Địa";

    [ObservableProperty]
    private string accessDeniedDescription = "Tính năng Chatbot AI dành cho người dùng Tour Basic hoặc Tour Plus để tư vấn ẩm thực đường phố.";

    [ObservableProperty]
    private string upgradeButtonText = "Nâng cấp ngay";

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ChatViewModel(AiService aiService, AudioService audioService)
    {
        _aiService = aiService;
        _audioService = audioService;
        _audioService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedTexts();
        AddWelcomeMessage();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        if (IsLoading) return;
        IsLoading = true;

        try
        {
            var info = await _aiService.GetSubscriptionInfoAsync();
            if (info == null || !info.AllowChatbot)
            {
                IsAccessDenied = true;
                return;
            }

            IsAccessDenied = false;
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
    public async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(UserMessage) || IsLoading) return;

        var messageText = UserMessage;
        UserMessage = string.Empty;

        Messages.Add(new ChatMessage { Text = messageText, IsFromUser = true });
        
        IsLoading = true;
        try
        {
            // Build history from last 10 messages
            var history = Messages
                .Where(m => !string.IsNullOrEmpty(m.Text))
                .TakeLast(11) // excluding the current one just added
                .Select(m => new AiChatTurnDto
                {
                    Role = m.IsFromUser ? "user" : "model",
                    Message = m.Text
                })
                .SkipLast(1) // remove the one we just added manually
                .ToList();

            var response = await _aiService.ChatWithThoDiaAsync(new AiChatbotRequestDto
            {
                Message = messageText,
                History = history,
                Language = _audioService.CurrentLanguage
            });

            if (response != null && response.Success)
            {
                Messages.Add(new ChatMessage { Text = response.Reply ?? GetText("fallback_no_reply"), IsFromUser = false });
            }
            else
            {
                Messages.Add(new ChatMessage { Text = GetText("fallback_busy"), IsFromUser = false });
            }
        }
        catch (Exception)
        {
            Messages.Add(new ChatMessage { Text = GetText("fallback_error"), IsFromUser = false });
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

    private void OnLanguageChanged(object? sender, string language)
    {
        UpdateLocalizedTexts();
    }

    private void AddWelcomeMessage()
    {
        Messages.Clear();
        Messages.Add(new ChatMessage
        {
            Text = GetText("welcome"),
            IsFromUser = false
        });
    }

    private void UpdateLocalizedTexts()
    {
        PageTitle = GetText("page_title");
        OnlineStatus = GetText("online_status");
        InputPlaceholder = GetText("input_placeholder");
        TypingIndicator = GetText("typing_indicator");
        AccessDeniedTitle = GetText("access_denied_title");
        AccessDeniedDescription = GetText("access_denied_description");
        UpgradeButtonText = GetText("upgrade_button");
    }

    private string GetText(string key)
    {
        var lang = _audioService.CurrentLanguage;
        return (lang, key) switch
        {
            ("en", "page_title") => "Tho Dia Vinh Khanh",
            ("en", "online_status") => "Online",
            ("en", "input_placeholder") => "Ask Tho Dia anything...",
            ("en", "typing_indicator") => "Tho Dia is typing...",
            ("en", "access_denied_title") => "Chat with Tho Dia",
            ("en", "access_denied_description") => "The AI Chatbot feature is available for Tour Basic or Tour Plus users to get street food recommendations.",
            ("en", "upgrade_button") => "Upgrade now",
            ("en", "welcome") => "Hello! I'm Tho Dia Vinh Khanh. What delicious food are you looking for today?",
            ("en", "fallback_no_reply") => "Tho Dia doesn't know what to say...",
            ("en", "fallback_busy") => "Sorry, I'm a bit busy right now. Please try again later!",
            ("en", "fallback_error") => "An error occurred while connecting to Tho Dia.",

            ("zh", "page_title") => "土地公永庆",
            ("zh", "online_status") => "在线",
            ("zh", "input_placeholder") => "问土地公任何问题...",
            ("zh", "typing_indicator") => "土地公正在输入...",
            ("zh", "access_denied_title") => "与土地公聊天",
            ("zh", "access_denied_description") => "AI聊天机器人功能面向Tour Basic或Tour Plus用户，提供街头美食推荐。",
            ("zh", "upgrade_button") => "立即升级",
            ("zh", "welcome") => "你好！我是土地公永庆。你今天想找什么好吃的？",
            ("zh", "fallback_no_reply") => "土地公不知道说什么了...",
            ("zh", "fallback_busy") => "抱歉，我现在有点忙。请稍后再试！",
            ("zh", "fallback_error") => "连接土地公时发生错误。",

            (_, "page_title") => "Thổ Địa Vĩnh Khánh",
            (_, "online_status") => "Đang trực tuyến",
            (_, "input_placeholder") => "Hỏi Thổ Địa cái gì hay ho...",
            (_, "typing_indicator") => "Thổ Địa đang gõ...",
            (_, "access_denied_title") => "Trò chuyện với Thổ Địa",
            (_, "access_denied_description") => "Tính năng Chatbot AI dành cho người dùng Tour Basic hoặc Tour Plus để tư vấn ẩm thực đường phố.",
            (_, "upgrade_button") => "Nâng cấp ngay",
            (_, "welcome") => "Chào bạn! Tôi là Thổ Địa Vĩnh Khánh đây. Bạn muốn tìm món gì ngon hôm nay?",
            (_, "fallback_no_reply") => "Thổ Địa không biết nói gì luôn...",
            (_, "fallback_busy") => "Xin lỗi, tôi đang bận chút việc. Bạn thử lại sau nhé!",
            (_, "fallback_error") => "Đã có lỗi xảy ra khi kết nối với Thổ Địa.",
            _ => key
        };
    }
}
