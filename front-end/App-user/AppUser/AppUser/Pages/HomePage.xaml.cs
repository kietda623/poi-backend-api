using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class HomePage : ContentPage
    {
        private readonly HomeViewModel _vm;
        private bool _isInitializing;

        public HomePage(HomeViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_isInitializing) return;

            try
            {
                _isInitializing = true;
                await _vm.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HomePage.OnAppearing error: {ex}");
            }
            finally
            {
                _isInitializing = false;
            }
        }
    }
}
