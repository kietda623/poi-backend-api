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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _vm.Initialize();
        }

        // Floating chatbot bubble tap handler
        private async void OnChatbotTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("chat");
        }
    }
}
