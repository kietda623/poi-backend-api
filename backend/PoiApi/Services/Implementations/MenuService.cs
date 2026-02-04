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

    // ADMIN / OWNER
    public async Task<List<MenuAdminDto>> GetByShopAsync(int shopId)
    {
        return await _context.Menus
            .Where(m => m.ShopId == shopId)
            .Select(m => new MenuAdminDto
            {
                Id = m.Id,
                Name = m.Name
            })
            .ToListAsync();
    }

    // ADMIN / OWNER
    public async Task<MenuAdminDto?> CreateAsync(int shopId, CreateMenuDto dto)
    {
        var shop = await _context.Shops.FindAsync(shopId);
        if (shop == null) return null;

        var menu = new Menu
        {
            Name = dto.Name,
            ShopId = shopId
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
