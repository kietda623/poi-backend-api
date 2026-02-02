using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;

public interface IMenuItemService
{
    Task<List<MenuItemAdminDto>> GetByMenuAsync(int menuId);
    Task<MenuItemAdminDto?> CreateAsync(int menuId, CreateMenuItemDto dto);
}
