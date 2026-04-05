using AppUser.ViewModels;

namespace AppUser.Pages
{
    public partial class POIListPage : ContentPage
    {
        private readonly POIListViewModel _vm;

        public POIListPage(POIListViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            BindingContext = vm;

            LoadMapHtml();
            _vm.FilteredPOIs.CollectionChanged += (s, e) => UpdateMapData();
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(POIListViewModel.UserLocation))
                {
                    UpdateMapData();
                }
            };
        }

        private async void LoadMapHtml()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("MapWebView.html");
                using var reader = new System.IO.StreamReader(stream);
                var html = await reader.ReadToEndAsync();
                MapWebView.Source = new HtmlWebViewSource { Html = html };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi tải HTML Bản đồ: {ex.Message}");
            }
        }

        private void MapWebView_Navigated(object sender, WebNavigatedEventArgs e)
        {
            UpdateMapData();
        }

        private void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("play-poi:"))
            {
                e.Cancel = true; 
                var idStr = e.Url.Split(':')[1];
                if (int.TryParse(idStr, out int poiId))
                {
                    _vm.PlayPOIByIdCommand.Execute(poiId);
                }
            }
        }

        private async void UpdateMapData()
        {
            if (_vm.FilteredPOIs == null || !_vm.FilteredPOIs.Any()) return;

            try
            {
                var poiList = _vm.FilteredPOIs.Select(p => new
                {
                    Id = p.Id,
                    Lat = p.Latitude,
                    Lng = p.Longitude,
                    Name = string.IsNullOrEmpty(p.Shop?.Name) ? "POI" : p.Shop.Name.Replace("'", "\\'"),
                    Address = string.IsNullOrEmpty(p.Location) ? "" : p.Location.Replace("'", "\\'")
                }).ToList();

                string json = System.Text.Json.JsonSerializer.Serialize(poiList);

                // Pass data to Javascript
                await MapWebView.EvaluateJavaScriptAsync($"updatePOIs('{json}');");
                
                if (_vm.UserLocation != null)
                {
                    await MapWebView.EvaluateJavaScriptAsync($"updateUserLocation({_vm.UserLocation.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {_vm.UserLocation.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)});");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi truyền dữ liệu Map: {ex.Message}");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.InitializeAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _vm.StopTracking();
        }
    }
}
