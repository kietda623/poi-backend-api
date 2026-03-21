using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class CategoryService
{
    private static List<CategoryModel> _mockCategories = new()
    {
        new() { Id=1, Name="Ốc", Slug="oc", StoreCount=15, IsActive=true },
        new() { Id=2, Name="Hải sản", Slug="hai-san", StoreCount=8, IsActive=true },
        new() { Id=3, Name="Cơm", Slug="com", StoreCount=12, IsActive=true },
        new() { Id=4, Name="Bún/Phở", Slug="bun-pho", StoreCount=20, IsActive=true },
        new() { Id=5, Name="Đồ uống", Slug="do-uong", StoreCount=5, IsActive=true }
    };

    public async Task<List<CategoryModel>> GetCategoriesAsync()
    {
        await Task.Delay(100);
        return _mockCategories.ToList();
    }

    public async Task<(bool, string)> CreateCategoryAsync(CategoryModel model)
    {
        await Task.Delay(200);
        model.Id = _mockCategories.Count > 0 ? _mockCategories.Max(x => x.Id) + 1 : 1;
        model.StoreCount = 0;
        _mockCategories.Add(model);
        return (true, "Thêm danh mục thành công!");
    }

    public async Task<(bool, string)> UpdateCategoryAsync(CategoryModel model)
    {
        await Task.Delay(200);
        var idx = _mockCategories.FindIndex(c => c.Id == model.Id);
        if (idx < 0) return (false, "Không tìm thấy danh mục!");
        _mockCategories[idx] = model;
        return (true, "Cập nhật thành công!");
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        await Task.Delay(100);
        var item = _mockCategories.FirstOrDefault(x => x.Id == id);
        if (item != null) _mockCategories.Remove(item);
        return true;
    }
}
