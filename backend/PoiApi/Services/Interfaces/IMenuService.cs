using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;

public interface IMenuService
{
    Task<List<MenuAdminDto>> GetByShopAsync(int shopId);
    Task<MenuAdminDto?> CreateAsync(int shopId, CreateMenuDto dto);
}

