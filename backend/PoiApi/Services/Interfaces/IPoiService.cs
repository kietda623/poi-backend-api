using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;

public interface IPoiService
{
    Task<List<POIAdminDto>> GetAllAsync(string lang);
    Task<POIAdminDto?> GetByIdAsync(int id, string lang);
    Task<POIAdminDto> CreateAsync(CreatePoiDto dto);
}
