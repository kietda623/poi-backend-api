using foodstreet_admin.Models;

namespace foodstreet_admin.Services;

public class TourService
{
    private static List<TourModel> _mockTours = new()
    {
        new() { Id=1, Name="Tour Ẩm thực Đường phố", Description="Khám phá các món ăn vặt", EstimatedTime="1 giờ 30 phút", IsActive=true, StoreIds=new List<int>{1, 4} },
        new() { Id=2, Name="Hải sản Vĩnh Khánh", Description="Thưởng thức ốc và hải sản tươi sống", EstimatedTime="2 giờ", IsActive=true, StoreIds=new List<int>{1, 3} }
    };

    public async Task<List<TourModel>> GetToursAsync()
    {
        await Task.Delay(100);
        return _mockTours.ToList();
    }

    public async Task<(bool, string)> CreateTourAsync(TourModel model)
    {
        await Task.Delay(200);
        model.Id = _mockTours.Count > 0 ? _mockTours.Max(x => x.Id) + 1 : 1;
        _mockTours.Add(model);
        return (true, "Thêm Tour thành công!");
    }

    public async Task<(bool, string)> UpdateTourAsync(TourModel model)
    {
        await Task.Delay(200);
        var idx = _mockTours.FindIndex(c => c.Id == model.Id);
        if (idx < 0) return (false, "Không tìm thấy Tour!");
        _mockTours[idx] = model;
        return (true, "Cập nhật thành công!");
    }

    public async Task<bool> DeleteTourAsync(int id)
    {
        await Task.Delay(100);
        var item = _mockTours.FirstOrDefault(x => x.Id == id);
        if (item != null) _mockTours.Remove(item);
        return true;
    }
}
