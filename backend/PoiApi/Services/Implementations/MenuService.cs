using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.Models;

public class MenuService : IMenuService
{
    private readonly AppDbContext _context;

    public MenuService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuAdminDto>> GetByPoiAsync(int poiId)
    {
        return await _context.Menus
            .Where(m => m.PoiId == poiId)
            .Select(m => new MenuAdminDto
            {
                Id = m.Id,
                Name = m.Name
            })
            .ToListAsync();
    }

    public async Task<MenuAdminDto?> CreateAsync(int poiId, CreateMenuDto dto)
    {
        var poiExists = await _context.POIs.AnyAsync(p => p.Id == poiId);
        if (!poiExists) return null;

        var menu = new Menu
        {
            Name = dto.Name,
            PoiId = poiId
        };

        _context.Menus.Add(menu);
        await _context.SaveChangesAsync();

        return new MenuAdminDto
        {
            Id = menu.Id,
            Name = menu.Name
        };
    }
}
