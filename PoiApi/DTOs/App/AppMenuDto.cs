namespace PoiApi.DTOs.App
{
    public class AppMenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public List<AppMenuItemDto> Items { get; set; } = new();
    }
}
