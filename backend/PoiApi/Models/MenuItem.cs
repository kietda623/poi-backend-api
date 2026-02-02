namespace PoiApi.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }

        public Menu Menu { get; set; } = null!;
    }

}
