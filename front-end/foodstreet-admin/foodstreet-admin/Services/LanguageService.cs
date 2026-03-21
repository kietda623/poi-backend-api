using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class LanguageService
{
    private static List<LanguageModel> _mockLanguages = new()
    {
        new() { Id=1, Code="vi", Name="Tiếng Việt", IsActive=true },
        new() { Id=2, Code="en", Name="English", IsActive=true },
        new() { Id=3, Code="ko", Name="한국어 (Korean)", IsActive=false },
        new() { Id=4, Code="ja", Name="日本語 (Japanese)", IsActive=false }
    };

    public async Task<List<LanguageModel>> GetLanguagesAsync()
    {
        await Task.Delay(100);
        return _mockLanguages.ToList();
    }

    public async Task<(bool, string)> CreateLanguageAsync(LanguageModel model)
    {
        await Task.Delay(200);
        model.Id = _mockLanguages.Count > 0 ? _mockLanguages.Max(x => x.Id) + 1 : 1;
        _mockLanguages.Add(model);
        return (true, "Thêm ngôn ngữ thành công!");
    }

    public async Task<(bool, string)> UpdateLanguageAsync(LanguageModel model)
    {
        await Task.Delay(200);
        var idx = _mockLanguages.FindIndex(l => l.Id == model.Id);
        if (idx < 0) return (false, "Không tìm thấy ngôn ngữ!");
        _mockLanguages[idx] = model;
        return (true, "Cập nhật thành công!");
    }

    public async Task<bool> DeleteLanguageAsync(int id)
    {
        await Task.Delay(100);
        var item = _mockLanguages.FirstOrDefault(x => x.Id == id);
        if (item != null) _mockLanguages.Remove(item);
        return true;
    }
}
