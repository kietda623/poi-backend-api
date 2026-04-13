using AppUser.ViewModels;

namespace AppUser.Pages;

public partial class ChatPage : ContentPage
{
    private readonly ChatViewModel _viewModel;
    private bool _isInitializing;

    public ChatPage(ChatViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_isInitializing) return;

        try
        {
            _isInitializing = true;
            await _viewModel.InitializeAsync();

            // Safely scroll to last message if available
            var lastMessage = _viewModel.Messages.LastOrDefault();
            if (lastMessage != null)
            {
                MsgList.ScrollTo(lastMessage, position: ScrollToPosition.End, animate: false);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ChatPage.OnAppearing error: {ex}");
        }
        finally
        {
            _isInitializing = false;
        }
    }
}
