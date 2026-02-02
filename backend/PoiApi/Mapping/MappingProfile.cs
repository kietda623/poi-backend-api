using AutoMapper;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.DTOs.App;
using PoiApi.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace PoiApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ===== ADMIN =====
        CreateMap<POI, POIAdminDto>();
        CreateMap<Menu, MenuAdminDto>();
        CreateMap<MenuItem, MenuItemAdminDto>();

        // ===== APP =====
        CreateMap<POI, AppPoiListDto>();

        CreateMap<POI, AppPoiDetailDto>()
            .ForMember(dest => dest.Menus,
                opt => opt.MapFrom(src => src.Menus));

        CreateMap<Menu, AppMenuDto>()
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.MenuItems));

        CreateMap<MenuItem, AppMenuItemDto>();
    }
}
