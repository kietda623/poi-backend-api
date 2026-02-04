namespace PoiApi.DTOs.Owner.Menu
{
    public class CreateMenuDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int DisplayOrder { get; set; } = 0;
    }
}
