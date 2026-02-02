namespace PoiApi.DTOs.App
{
    public class AppPoiListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; } = null!;
        public string Location { get; set; } = null!;
    }
}
