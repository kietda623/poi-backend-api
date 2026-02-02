namespace PoiApi.DTOs.App
{
    public class AppPoiDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string Location { get; set; } = null!;
        public List<AppMenuDto> Menus { get; set; } = new();
    }
}
