namespace PoiApi.DTOs.Owner
{
    public class ShopCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? MenuImagesUrl { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
