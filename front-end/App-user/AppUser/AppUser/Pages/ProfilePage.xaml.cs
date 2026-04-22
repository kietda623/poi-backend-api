using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class ProfilePage : ContentPage
    {
        private readonly ProfileViewModel _vm;

        public ProfilePage(ProfileViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitializeAsync();
        }

        private async void OnChatbotTapped(object? sender, TappedEventArgs e)
        {
            await Shell.Current.GoToAsync("chat");
        }
    }
}
