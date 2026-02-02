using Microsoft.EntityFrameworkCore;
using PoiApi.Data;
using PoiApi.DTOs.Admin.Requests;
using PoiApi.DTOs.Admin.Responses;
using PoiApi.Models;

public class MenuItemService : IMenuItemService
{
    private readonly AppDbContext _context;

    public MenuItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MenuItemAdminDto>> GetByMenuAsync(int menuId)
    {
        return await _context.MenuItems
            .Where(i => i.MenuId == menuId)
            .Select(i => new MenuItemAdminDto
            {
                Id = i.Id,
                Name = i.Name,
                Price = i.Price
            })
            .ToListAsync();
    }

    public async Task<MenuItemAdminDto?> CreateAsync(int menuId, CreateMenuItemDto dto)
    {
        var menuExists = await _context.Menus.AnyAsync(m => m.Id == menuId);
        if (!menuExists) return null;

        var item = new MenuItem
        {
            MenuId = menuId,
            Name = dto.Name,
            Price = dto.Price
        };

        _context.MenuItems.Add(item);
        await _context.SaveChangesAsync();

        return new MenuItemAdminDto
        {
            Id = item.Id,
            Name = item.Name,
            Price = item.Price
        };
    }
}
