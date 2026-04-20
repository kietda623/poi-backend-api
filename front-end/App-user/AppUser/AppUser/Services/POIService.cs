using AppUser.Models;
using System.Net.Http.Json;
using System.Diagnostics;
using System.Text.Json;

namespace AppUser.Services
{
    public class POIService
    {
        private readonly HttpClient _http;
        private readonly AuthService _authService;

        public POIService(AuthService authService)
        {
            _authService = authService;
            _http = new HttpClient
            {
                BaseAddress = new Uri(AppConfig.BaseApiUrl)
            };
        }

        public async Task<List<POIDto>> GetAllPOIsAsync(string lang = "vi")
        {
            try
            {
                ApplyAuthorizationHeaderIfAvailable();
                var response = await _http.GetFromJsonAsync<List<AppPoiListDto>>($"app/pois?lang={lang}");
                if (response == null) return new();

                return response.Select(x => MapToListDto(x, lang)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching POIs: {ex.Message}");
                return new();
            }
        }

        public async Task<List<POIDto>> GetFeaturedPOIsAsync(int count = 3, string lang = "vi")
        {
            var all = await GetAllPOIsAsync(lang);
            return all.Take(count).ToList();
        }

        public async Task<POIDto?> GetPOIByIdAsync(int id, string lang = "vi")
        {
            try
            {
                ApplyAuthorizationHeaderIfAvailable();
                var response = await _http.GetFromJsonAsync<AppPoiDetailDto>($"app/pois/{id}?lang={lang}");
                if (response == null) return null;

                return MapToDetailDto(response, lang);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching POI detail: {ex.Message}");
                return null;
            }
        }

        public async Task<List<POIDto>> SearchPOIsAsync(string query, string lang = "vi")
        {
            try
            {
                ApplyAuthorizationHeaderIfAvailable();
                var response = await _http.GetFromJsonAsync<List<AppPoiListDto>>($"app/pois?lang={lang}&search={query}");
                if (response == null) return new();

                return response.Select(x => MapToListDto(x, lang)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching POIs: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> TrackViewAsync(int poiId)
        {
            try
            {
                ApplyAuthorizationHeaderIfAvailable();
                var response = await _http.PostAsync($"app/pois/{poiId}/view", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error tracking view: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TrackListenAsync(int poiId)
        {
            try
            {
                ApplyAuthorizationHeaderIfAvailable();
                var deviceId = DeviceInfo.Current.Idiom.ToString() + "_" + DeviceInfo.Current.Platform.ToString();
                var response = await _http.PostAsync($"app/pois/{poiId}/listen?deviceId={deviceId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error tracking listen: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SubmitReviewAsync(int poiId, int rating, string comment, string customerName = "Khách hàng")
            => (await SubmitReviewWithResultAsync(poiId, rating, comment, customerName)).Success;

        public async Task<(bool Success, string? Message)> SubmitReviewWithResultAsync(
            int poiId,
            int rating,
            string comment,
            string customerName = "Khách hàng")
        {
            try
            {
                ApplyAuthorizationHeaderIfAvailable();
                var dto = new AppReviewDto
                {
                    Rating = rating,
                    Comment = comment,
                    CustomerName = _authService.CurrentUser?.FullName ?? customerName
                };
                var response = await _http.PostAsJsonAsync($"app/pois/{poiId}/reviews", dto);
                if (response.IsSuccessStatusCode) return (true, null);

                var responseText = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var parsedMessage = TryExtractMessage(responseText);
                    return (false, parsedMessage ?? "Bạn cần nghe POI trước khi gửi đánh giá.");
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (false, "Bạn cần đăng nhập để gửi đánh giá.");
                }

                return (false, TryExtractMessage(responseText) ?? "Không thể gửi đánh giá.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error submitting review: {ex.Message}");
                return (false, "Không thể kết nối đến máy chủ.");
            }
        }

        private static string? TryExtractMessage(string responseText)
        {
            if (string.IsNullOrWhiteSpace(responseText)) return null;
            try
            {
                var payload = JsonSerializer.Deserialize<ApiErrorPayload>(responseText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return payload?.Message;
            }
            catch
            {
                return null;
            }
        }

        private sealed class ApiErrorPayload
        {
            public string? Message { get; set; }
        }

        // Áp dụng token vào header: ưu tiên User Token, fallback sang Guest Token (từ AuthService)
        private void ApplyAuthorizationHeaderIfAvailable()
        {
            var token = _authService.Token; // AuthService tự fallback sang Guest Token
            if (string.IsNullOrWhiteSpace(token))
            {
                _http.DefaultRequestHeaders.Authorization = null;
                return;
            }

            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        private POIDto MapToListDto(AppPoiListDto api, string lang)
        {
            var poi = new POIDto
            {
                Id = api.Id,
                ImageUrl = AppConfig.ResolveUrl(api.ImageUrl),
                Location = api.Location,
                Latitude = api.Latitude,
                Longitude = api.Longitude,
                Shop = new ShopDto { Name = api.Name }
            };

            // Add a single audio guide if exists
            if (!string.IsNullOrEmpty(api.AudioUrl))
            {
                poi.AudioGuides.Add(new AudioGuideDto
                {
                    POIId = api.Id,
                    AudioUrl = AppConfig.ResolveUrl(api.AudioUrl),
                    Title = "Thuyết minh " + api.Name,
                    LanguageCode = lang,
                    DurationSeconds = 120 // Default placeholder
                });
            }

            return poi;
        }

        private POIDto MapToDetailDto(AppPoiDetailDto api, string lang)
        {
            var poi = new POIDto
            {
                Id = api.Id,
                ImageUrl = AppConfig.ResolveUrl(api.ImageUrl),
                Location = api.Location,
                Latitude = api.Latitude,
                Longitude = api.Longitude,
                Shop = new ShopDto 
                { 
                    Name = api.Name,
                    Description = api.Description
                }
            };

            // Population Translations with the current localized content from API
            poi.Translations.Add(new POITranslationDto
            {
                POIId = api.Id,
                Name = api.Name,
                Description = api.Description,
                LanguageCode = lang
            });

            if (!string.IsNullOrEmpty(api.AudioUrl))
            {
                poi.AudioGuides.Add(new AudioGuideDto
                {
                    POIId = api.Id,
                    AudioUrl = AppConfig.ResolveUrl(api.AudioUrl),
                    Title = "Thuyết minh " + api.Name,
                    LanguageCode = lang
                });
            }

            poi.ImageUrl = AppConfig.ResolveUrl(api.ImageUrl);
            poi.MenuImagesUrl = api.MenuImagesUrl;
            
            if (!string.IsNullOrEmpty(api.MenuImagesUrl))
            {
                poi.MenuImages = api.MenuImagesUrl.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(url => AppConfig.ResolveUrl(url.Trim()))
                    .ToList();
            }

            poi.AvailableLanguages = api.AvailableLanguages;

            // Map Menus and MenuItems
            if (api.Menus != null)
            {
                poi.Shop.Menus = api.Menus.Select(m => new MenuDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Items = m.Items.Select(i => new MenuItemDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        Price = i.Price,
                        IsAvailable = i.IsAvailable,
                        ImageUrl = AppConfig.ResolveUrl(i.ImageUrl)
                    }).ToList()
                }).ToList();
            }

            return poi;
        }
    }

}
