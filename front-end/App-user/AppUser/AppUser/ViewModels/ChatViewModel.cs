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

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool isAccessDenied;

    [ObservableProperty]
    private string userMessage = string.Empty;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ChatViewModel(AiService aiService)
    {
        _aiService = aiService;
        
        // Initial welcome message
        Messages.Add(new ChatMessage 
        { 
            Text = "Chào bạn! Tôi là Thổ Địa Vĩnh Khánh đây. Bạn muốn tìm món gì ngon hôm nay?", 
            IsFromUser = false 
        });
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
                History = history
            });

            if (response != null && response.Success)
            {
                Messages.Add(new ChatMessage { Text = response.Reply ?? "Thổ Địa không biết nói gì luôn...", IsFromUser = false });
            }
            else
            {
                Messages.Add(new ChatMessage { Text = "Xin lỗi, tôi đang bận chút việc. Bạn thử lại sau nhé!", IsFromUser = false });
            }
        }
        catch (Exception)
        {
            Messages.Add(new ChatMessage { Text = "Đã có lỗi xảy ra khi kết nối với Thổ Địa.", IsFromUser = false });
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
}
