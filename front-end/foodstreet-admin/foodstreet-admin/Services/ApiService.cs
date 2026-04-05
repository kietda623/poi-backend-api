    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text.Json;

    namespace foodstreet_admin.Services;

    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly TokenService _tokenService;
        private readonly ILogger<ApiService> _logger;
        private readonly Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider _authState;
        private readonly UILanguageService _uiLang;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, TokenService tokenService, ILogger<ApiService> logger, 
                         Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider authState,
                         UILanguageService uiLang)
        {
            _http = http;
            _tokenService = tokenService;
            _logger = logger;
            _authState = authState;
            _uiLang = uiLang;
        }

        // Đính Bearer token và Accept-Language vào header trước mỗi request
        private async Task ApplyHeadersAsync()
        {
            // Set Language Header
            _http.DefaultRequestHeaders.AcceptLanguage.Clear();
            _http.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(_uiLang.CurrentLanguage));

            if (!_tokenService.HasToken)
            {
                try 
                {
                    var authState = await _authState.GetAuthenticationStateAsync();
                    var jwtClaim = authState.User.FindFirst("jwt_token");
                    if (jwtClaim != null)
                        _tokenService.SetToken(jwtClaim.Value);
                }
                catch { }
            }

            if (_tokenService.HasToken)
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _tokenService.Token);
            else
                _http.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                await ApplyHeadersAsync();
                var response = await _http.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GET {Endpoint} failed", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                await ApplyHeadersAsync();
                var response = await _http.PostAsJsonAsync(endpoint, data);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("POST {Endpoint} returned {StatusCode}: {ErrorBody}", endpoint, response.StatusCode, errorBody);
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST {Endpoint} failed", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PostMultipartAsync<TResponse>(string endpoint, Stream fileStream, string fileName)
        {
            try
            {
                await ApplyHeadersAsync();
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Adjust based on file extension
                content.Add(fileContent, "file", fileName);

                var response = await _http.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("POST Multipart {Endpoint} returned {StatusCode}: {ErrorBody}", endpoint, response.StatusCode, errorBody);
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST Multipart {Endpoint} failed", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PostMultipartMultipleAsync<TResponse>(string endpoint, List<(Stream Stream, string FileName)> files)
        {
            try
            {
                await ApplyHeadersAsync();
                using var content = new MultipartFormDataContent();
                foreach (var file in files)
                {
                    var fileContent = new StreamContent(file.Stream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    content.Add(fileContent, "files", file.FileName);
                }

                var response = await _http.PostAsync(endpoint, content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("POST Multipart {Endpoint} returned {StatusCode}: {ErrorBody}", endpoint, response.StatusCode, errorBody);
                    return default;
                }

                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "POST Multiple Multipart {Endpoint} failed", endpoint);
                return default;
            }
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                await ApplyHeadersAsync();
                var response = await _http.PutAsJsonAsync(endpoint, data);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PUT {Endpoint} failed", endpoint);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                await ApplyHeadersAsync();
                var response = await _http.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DELETE {Endpoint} failed", endpoint);
                return false;
            }
        }

        public async Task<bool> PatchAsync(string endpoint)
        {
            try
            {
                await ApplyHeadersAsync();
                var response = await _http.PatchAsync(endpoint, null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PATCH {Endpoint} failed", endpoint);
                return false;
            }
        }
    }
