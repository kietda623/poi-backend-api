using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AppUser.Models;
using AppUser.Services;
using System.Collections.ObjectModel;

namespace AppUser.ViewModels
{
    public partial class POIListViewModel : ObservableObject
    {
        private readonly POIService _poiService;
        private readonly AudioService _audioService;
        private readonly AuthService _authService;
        private readonly SubscriptionService _subscriptionService;
        private List<POIDto> _allPOIs = new();
        
        private IDispatcherTimer? _locationTimer;
        private readonly HashSet<int> _notifiedPoiIds = new();

        [ObservableProperty]
        private ObservableCollection<POIDto> filteredPOIs = new();

        [ObservableProperty]
        private string searchQuery = string.Empty;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private bool isEmpty = false;

        [ObservableProperty]
        private bool isMapView = false;

        [ObservableProperty]
        private Location? userLocation;

        [ObservableProperty]
        private string currentLanguage = "vi";

        public POIListViewModel(POIService poi, AudioService audio, AuthService authService, SubscriptionService subscriptionService)
        {
            _poiService = poi;
            _audioService = audio;
            _authService = authService;
            _subscriptionService = subscriptionService;
            CurrentLanguage = _audioService.CurrentLanguage;
        }

        public async Task InitializeAsync()
        {
            await LoadAllPOIsAsync();
            await RequestLocationPermissionAsync();
            StartLocationTracking();
        }

        private void StartLocationTracking()
        {
            if (_locationTimer != null) return;
            
            _locationTimer = Application.Current!.Dispatcher.CreateTimer();
            _locationTimer.Interval = TimeSpan.FromSeconds(10); // check every 10 seconds
            _locationTimer.Tick += async (s, e) => await CheckProximityAsync();
            _locationTimer.Start();
        }

        public void StopTracking()
        {
            _locationTimer?.Stop();
            _locationTimer = null;
        }

        private async Task CheckProximityAsync()
        {
            if (UserLocation == null || _allPOIs.Count == 0) return;

            try
            {
                var loc = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5)));
                if (loc != null) UserLocation = loc;
            }
            catch { /* Ignore */ }

            if (UserLocation == null) return;

            // Find nearest POI within 100 meters
            var nearestPoi = _allPOIs
                .Where(p => p.Latitude.HasValue && p.Longitude.HasValue && !_notifiedPoiIds.Contains(p.Id))
                .Select(p => new 
                { 
                    Poi = p, 
                    Distance = Location.CalculateDistance(
                        UserLocation.Latitude, UserLocation.Longitude, 
                        p.Latitude!.Value, p.Longitude!.Value, 
                        DistanceUnits.Kilometers) * 1000 // meters
                })
                .Where(x => x.Distance <= 100) // Within 100 meters
                .OrderBy(x => x.Distance)
                .FirstOrDefault();

            if (nearestPoi != null)
            {
                _notifiedPoiIds.Add(nearestPoi.Poi.Id);
                
                // Prompt user
                Application.Current?.Dispatcher.Dispatch(async () =>
                {
                    bool wantToListen = await Shell.Current.DisplayAlert(
                        "Điểm ẩm thực gần bạn!",
                        $"Bạn đang ở rất gần {nearestPoi.Poi.Shop?.Name}. Bạn có muốn nghe thuyết minh không?",
                        "Nghe ngay",
                        "Bỏ qua"
                    );

                    if (wantToListen)
                    {
                        await NavigateToPOIAsync(nearestPoi.Poi);
                    }
                });
            }
        }

        private async Task RequestLocationPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                }
                
                if (status == PermissionStatus.Granted)
                {
                    UserLocation = await Geolocation.Default.GetLastKnownLocationAsync()
                                ?? await Geolocation.Default.GetLocationAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting location: {ex.Message}");
            }
        }

        [RelayCommand]
        private void ToggleView()
        {
            IsMapView = !IsMapView;
        }

        [RelayCommand]
        private async Task LoadAllPOIsAsync()
        {
            IsLoading = true;
            try
            {
                _allPOIs = await _poiService.GetAllPOIsAsync(CurrentLanguage);
                ApplyFilter();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            IsLoading = true;
            try
            {
                var results = await _poiService.SearchPOIsAsync(SearchQuery, CurrentLanguage);
                FilteredPOIs.Clear();
                foreach (var p in results)
                    FilteredPOIs.Add(p);
                IsEmpty = !FilteredPOIs.Any();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToPOIAsync(POIDto poi)
        {
            await Shell.Current.GoToAsync("poiDetail",
                new Dictionary<string, object> { ["POI"] = poi });
        }

        [RelayCommand]
        private async Task PlayPOIById(int poiId)
        {
            var poi = _allPOIs.FirstOrDefault(p => p.Id == poiId);
            if (poi != null)
            {
                await NavigateToPOIAsync(poi);
            }
        }

        [RelayCommand]
        private async Task PlayAudioDirect(POIDto poi)
        {
            if (poi == null) return;

            if (!_authService.IsLoggedIn)
            {
                await Shell.Current.DisplayAlert("Dang nhap", "Ban can dang nhap de dang ky goi nghe thuyet minh.", "OK");
                await Shell.Current.GoToAsync("//login");
                return;
            }

            if (!await _subscriptionService.CanAccessAudioAsync())
            {
                var goToPackages = await Shell.Current.DisplayAlert("Can goi audio", "Ban can goi audio dang hoat dong de nghe thuyet minh.", "Dang ky goi", "De sau");
                if (goToPackages)
                {
                    await Shell.Current.GoToAsync("subscriptionPackages");
                }
                return;
            }

            // Fetch full detail to get audio URL
            var fullPoi = await _poiService.GetPOIByIdAsync(poi.Id, CurrentLanguage);
            if (fullPoi == null || !fullPoi.AudioGuides.Any())
            {
                await Shell.Current.DisplayAlert("Không có audio", "Điểm ẩm thực này chưa có thuyết minh audio.", "OK");
                return;
            }

            var guide = _audioService.GetGuideForPOI(fullPoi) ?? fullPoi.AudioGuides.First();
            _audioService.LoadGuide(guide, fullPoi);

            await Shell.Current.GoToAsync("audioPlayer",
                new Dictionary<string, object>
                {
                    ["AudioGuide"] = guide,
                    ["POI"] = fullPoi
                });
        }

        [RelayCommand]
        private async Task ToggleLanguageAsync()
        {
            CurrentLanguage = CurrentLanguage switch
            {
                "vi" => "en",
                "en" => "zh",
                _ => "vi"
            };
            
            _audioService.SetLanguage(CurrentLanguage);
            await LoadAllPOIsAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            SearchQuery = string.Empty;
            CurrentLanguage = _audioService.CurrentLanguage;
            await LoadAllPOIsAsync();
        }

        partial void OnSearchQueryChanged(string value)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var query = SearchQuery.ToLower().Trim();
            var filtered = string.IsNullOrEmpty(query)
                ? _allPOIs
                : _allPOIs.Where(p =>
                    p.DisplayName(CurrentLanguage).ToLower().Contains(query) ||
                    (p.Location?.ToLower().Contains(query) ?? false) ||
                    (p.Shop?.Name.ToLower().Contains(query) ?? false)).ToList();

            FilteredPOIs.Clear();
            foreach (var p in filtered)
                FilteredPOIs.Add(p);
            IsEmpty = !FilteredPOIs.Any();
        }
    }
}
