using AppUser.Models;
using System.Net.Http.Json;
using System.Diagnostics;

namespace AppUser.Services
{
    public class POIService
    {
        private readonly HttpClient _http;

        public POIService()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(AppConfig.BaseApiUrl)
            };
        }

        public async Task<List<POIDto>> GetAllPOIsAsync(string lang = "vi")
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<AppPoiListDto>>($"app/pois?lang={lang}");
                if (response == null) return new();

                return response.Select(MapToListDto).ToList();
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
                var response = await _http.GetFromJsonAsync<AppPoiDetailDto>($"app/pois/{id}?lang={lang}");
                if (response == null) return null;

                return MapToDetailDto(response);
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
                var response = await _http.GetFromJsonAsync<List<AppPoiListDto>>($"app/pois?lang={lang}&search={query}");
                if (response == null) return new();

                return response.Select(MapToListDto).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error searching POIs: {ex.Message}");
                return new();
            }
        }

        private POIDto MapToListDto(AppPoiListDto api)
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
                    LanguageCode = "vi",
                    DurationSeconds = 120 // Default placeholder
                });
            }

            return poi;
        }

        private POIDto MapToDetailDto(AppPoiDetailDto api)
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
                LanguageCode = string.Empty // This will be the "current" requested language
            });

            if (!string.IsNullOrEmpty(api.AudioUrl))
            {
                poi.AudioGuides.Add(new AudioGuideDto
                {
                    POIId = api.Id,
                    AudioUrl = AppConfig.ResolveUrl(api.AudioUrl),
                    Title = "Thuyết minh " + api.Name,
                    LanguageCode = "vi"
                });
            }

            poi.AvailableLanguages = api.AvailableLanguages;

            return poi;
        }
    }

}
