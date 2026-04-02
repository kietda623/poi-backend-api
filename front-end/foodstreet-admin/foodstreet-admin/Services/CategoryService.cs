using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class CategoryService
{
    private readonly ApiService _api;

    public CategoryService(ApiService api)
    {
        _api = api;
    }

    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        return await _api.GetAsync<List<CategoryModel>>("admin/categories") ?? new();
    }

    public async Task<(bool, string)> CreateCategoryAsync(CategoryModel model)
    {
        var result = await _api.PostAsync<CategoryModel, CategoryModel>("admin/categories", model);
        return result != null ? (true, "Thêm danh mục thành công!") : (false, "Lỗi khi thêm danh mục!");
    }

    public async Task<(bool, string)> UpdateCategoryAsync(CategoryModel model)
    {
        var result = await _api.PutAsync<CategoryModel, CategoryModel>($"admin/categories/{model.Id}", model);
        return result != null ? (true, "Cập nhật thành công!") : (false, "Lỗi khi cập nhật!");
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _api.DeleteAsync($"admin/categories/{id}");
    }
}
