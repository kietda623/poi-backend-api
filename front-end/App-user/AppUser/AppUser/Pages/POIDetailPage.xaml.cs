using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class POIDetailPage : ContentPage
    {
        private readonly POIDetailViewModel _vm;
        private bool _isInitializing;

        public POIDetailPage(POIDetailViewModel vm)
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
                System.Diagnostics.Debug.WriteLine($"POIDetailPage.OnAppearing error: {ex}");
            }
            finally
            {
                _isInitializing = false;
            }
        }
    }
}
