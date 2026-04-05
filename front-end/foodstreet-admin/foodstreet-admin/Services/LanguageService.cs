using System.Collections.Generic;
using System.Threading.Tasks;
using foodstreet_admin.Models;

namespace foodstreet_admin.Services
{
    public class LanguageService
    {
        private readonly ApiService _api;

        public LanguageService(ApiService api)
        {
            _api = api;
        }

        public async Task<List<LanguageModel>> GetLanguagesAsync()
        {
            return await _api.GetAsync<List<LanguageModel>>("admin/languages") ?? new();
        }

        public async Task<(bool Success, string Message)> CreateLanguageAsync(LanguageModel model)
        {
            var result = await _api.PostAsync<LanguageModel, LanguageModel>("admin/languages", model);
            return result != null ? (true, "Thêm ngôn ngữ thành công!") : (false, "Lỗi khi thêm ngôn ngữ!");
        }

        public async Task<(bool Success, string Message)> UpdateLanguageAsync(LanguageModel model)
        {
            var result = await _api.PutAsync<LanguageModel, LanguageModel>($"admin/languages/{model.Id}", model);
            return result != null ? (true, "Cập nhật thành công!") : (false, "Lỗi khi cập nhật!");
        }

        public async Task<bool> DeleteLanguageAsync(int id)
        {
            return await _api.DeleteAsync($"admin/languages/{id}");
        }
    }
}
