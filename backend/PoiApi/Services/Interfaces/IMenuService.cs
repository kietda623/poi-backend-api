using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;

public interface IMenuService
{
	Task<List<MenuAdminDto>> GetByPoiAsync(int poiId);
	Task<MenuAdminDto?> CreateAsync(int poiId, CreateMenuDto dto);
}
